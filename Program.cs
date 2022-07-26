using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using GvBot.RestModels;

namespace GvBot
{
    class Program
    {
        /// <summary>
        /// Клиент для общения с сервисом проверки подписи
        /// </summary>
        private static readonly HttpClient SvsCLient = new HttpClient();

        static async Task Main(string[] args)
        {
            // Зачитываем конфиг бота
            var config = await AppConfig.GetConfigAsync();

            // Создаём клиент для общения с api телеграмма
            var botClient = new TelegramBotClient(config.BotToken);
            using var cts = new CancellationTokenSource();
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            // создаём обработчик сообщений бота
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            TimedConsoleLogger.WriteLine($"Start listening for @{me.Username}");

            // Отсанавливаем бота при нажатии enter в консоли
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Message is not { } message)
                {
                    return;
                }

                // Не обрабатываем сообщений, которые пришли ранее, чем минуту назад
                if (update.Message.Date < DateTime.UtcNow - TimeSpan.FromMinutes(1))
                {
                    return;
                }

                var chatId = message.Chat.Id;

                // Игнорируем сообщения в целевом чате (в него только выдаём инвайты)
                if (chatId == config.PrivateChatId)
                {
                    return;
                }

                // Выводим привественное сообщение
                if (update.Message?.Text == "/start")
                {
                    Message error = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Приложите файл штампа о регистрации ДДУ в формате .xml",
                        cancellationToken: cancellationToken);

                    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : start");
                    return;
                }

                // игнорируем обработку любых сообщений, кроме вложенных файлов
                if (update.Message?.Document is not { } document)
                {
                    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : not a file message");
                    return;
                }
                var documentSize = message.Document.FileSize;
                if (documentSize > 100000)
                {
                    Message error = await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: $"Слишком большой файл",
                       cancellationToken: cancellationToken);
                    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : too big file [{documentSize}]");
                    return;
                }
                var fileId = message.Document.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;

                // Создаём поток для чтения в память
                using var stream = new MemoryStream();

                // Зачитываем файл в память
                await botClient.DownloadFileAsync(
                    filePath: filePath,
                    destination: stream);

                // Перематываем поток на начало, читаем и парсим их него Xml Документ
                stream.Position = 0;
                byte[] fileContenst = stream.ToArray();
                stream.Position = 0;

                try
                {
                    XDocument xd1 = new XDocument();
                    xd1 = XDocument.Load(stream);
                }
                catch (XmlException exception)
                {
                    Message error = await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: $"Некооректный документ",
                       cancellationToken: cancellationToken);
                    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : invalid doc");
                    return;
                }


                // В идеале лучше разобрать Xml файл и выкусить узел registration_number, но на текущем этапе просто ищем подстроку
                // в текстовом представлении файла
                var fileString = Encoding.UTF8.GetString(fileContenst);
                if (!fileString.Contains("77:05:0008007:14033-77/", StringComparison.OrdinalIgnoreCase))
                {
                    Message error = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Некооректный документ",
                        cancellationToken: cancellationToken);
                    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : doc not containing expected id");
                    return;
                }

                // Создаём запрос на проверку подписи
                var doc = new SignedDocument()
                {
                    SignatureType = SignatureType.XMLDSig,
                    Content = fileContenst
                };

                // Сериализуем в json
                var serializedDoc = System.Text.Json.JsonSerializer.Serialize(doc);

                // Выставляем тип содержимого и отправляем запрос
                var content = new StringContent(serializedDoc, Encoding.UTF8, "application/json");
                var result = await Program.SvsCLient.PostAsync($"https://dss.cryptopro.ru/Verify/rest/api/signatures", content);

                // Получаем ответ и разбираем результат
                // Нас интересует 2 возможных ответа - либо проверка полностью прошла, либо проверка не прошла,
                // но получили ответ "подпись математически корректна", что означет, что не удалось проверить корневые сертификаты
                // ибо к ним на проверящей стороне нет доверия. Так как мы сами проверим, что подписал кто то от росреестра - то не важно.
                var resultData = await result.Content.ReadAsStringAsync();

                VerificationResultRest verificationResult;

                try
                {
                    var verificationResults = JsonConvert.DeserializeObject<List<VerificationResultRest>>(resultData);

                    if (verificationResults.Count > 1)
                    {
                        // найдено 2 подписи. такое может быть, но не встречал. лучше пока ругаемся
                        Message error = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Не удалось распознать резульатат проверки",
                            cancellationToken: cancellationToken);
                        TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : invalid signature validation result");
                        return;
                    }

                    verificationResult = verificationResults[0];
                }

                
                catch (Exception)
                {
                    Message error = await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: $"Не удалось распознать резульатат проверки",
                           cancellationToken: cancellationToken);
                    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : invalid signature validation result");
                    return;
                    //// пробуем парсить как объект
                    //try
                    //{
                    //    verificationResult = JsonConvert.DeserializeObject<VerificationResultRest>(resultData);
                    //}
                    //catch (Exception)
                    //{
                    //    Message error = await botClient.SendTextMessageAsync(
                    //       chatId: chatId,
                    //       text: $"Не удалось распознать резульатат проверки",
                    //       cancellationToken: cancellationToken);
                    //    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : invalid signature validation result");
                    //    return;
                    //}
                }

                // Выкусываем подписанта, проверяем что в его имени есть росреестр
                // NB - да, возможная атака это создание своего самоподписанного сертификата, где в имени будет росреестр.
                // но предположим, что это за гранью возможностей рекламщиков ремонтов.
                if (verificationResult.SignerCertificateInfo == null ||
                    !verificationResult.SignerCertificateInfo[CertificateInfoParams.SubjectName].Contains("росреестр", StringComparison.OrdinalIgnoreCase))
                {
                    Message error = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Не удалось распознать подписанта",
                        cancellationToken: cancellationToken);
                    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : invalid signature subject " +
                        $"[{verificationResult.SignerCertificateInfo[CertificateInfoParams.SubjectName]}]");
                    return;
                }
                if (!verificationResult.Message.Contains("Подпись математически корректна", StringComparison.OrdinalIgnoreCase) &&
                    !verificationResult.Result)
                {
                    Message error = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Подпись некорректна",
                        cancellationToken: cancellationToken);
                    TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : invalid signature");
                    return;
                }


                // Генерим инвайт линк на 1 использование
                // NB. В идеале надо запоминать номер регистрации, id пользователя и инвайт.
                // Если такой номер уже был - просто выдавать старый инвайт, а не генерить новый.

                var inviteLink = await botClient.CreateChatInviteLinkAsync(
                    new ChatId(
                        config.PrivateChatId),
                    name: $"{update.Message.Chat.Username}_{DateTime.UtcNow.ToFileTimeUtc()}",
                    memberLimit: 1);

                TimedConsoleLogger.WriteLine($"user [{update.Message.Chat.Username}:{chatId}] : " +
                    $"new invite link generated [{inviteLink.Name} :: {inviteLink.InviteLink}]");

                // Отправляем инвайт
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"{inviteLink.InviteLink}",
                    cancellationToken: cancellationToken);
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                TimedConsoleLogger.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
        }
    }
}
