﻿using GreenVerticalBot.Bot;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Entities;
using GreenVerticalBot.RestModels;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GreenVerticalBot.Dialogs
{

    // 77:05:0008007:14033-77/060/2021-902
    // 77:05:0008007:14033 - участок
    // 

    internal class RegisterDialog : DialogBase
    {
        private RegisterDialogState state = RegisterDialogState.Initial;

        private readonly HttpClient svsCLient;

        private readonly IUserManager userManager;

        public RegisterDialog(
            IUserManager userManager,
            IHttpClientFactory httpClientFactory,
            DialogOrcestrator dialogOrcestrator,
            AppConfig config,
            ILogger<RegisterDialog> logger)
            : base(dialogOrcestrator, config, logger)
        {
            this.userManager = userManager ??
                throw new ArgumentNullException(nameof(userManager));

            this.svsCLient = httpClientFactory.CreateClient();
        }

        public override async Task ProcessUpdateCore(
            ITelegramBotClient telegramBotClient,
            Update update,
            CancellationToken cancellationToken)
        {
            switch (this.state)
            {
                case RegisterDialogState.Initial:
                {
                    var user = await this.userManager.GetUserByTelegramIdAsync(update.Message.From.Id);
                    if (user != null &&
                        user.Status != UserEntity.StatusFormats.New)
                    {
                        await telegramBotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text:
                                $"Пользователь уже зарегистрирован.{Environment.NewLine}" +
                                $"Используйте команду /user для просмотра информации",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                            cancellationToken: cancellationToken);

                        this.dialogOrcestrator.SwitchToDialog<WellcomeDialog>(
                            user.TelegramId.ToString(),
                             telegramBotClient,
                             update,
                             cancellationToken);
                        return;
                    }

                    var keyboard = new ReplyKeyboardMarkup(new KeyboardButton[][]
                    {
                        new[]
                        {
                            new KeyboardButton("/rosreestr Штамп о регистрации ДДУ в Росреестре"),
                        },
                        new[]
                        {
                            new KeyboardButton("/etc Прочий документ, подтверждающий владение"),
                        }
                    })
                    {
                        OneTimeKeyboard = true
                    };

                    var sentMessage = await telegramBotClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text:
                            $"Выберите способы регистации:{Environment.NewLine}{Environment.NewLine}" +
                            $" /rosteestr Штамп о регистрации ДДУ в Росреестре{Environment.NewLine}{Environment.NewLine}" +
                            $" /etc Прочий документ, подтверждающий владение",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken);
                    this.state = RegisterDialogState.SelectRegistrationType;
                    return;
                }
                case RegisterDialogState.SelectRegistrationType:
                {
                    if (update.Message.Text.StartsWith("/rosreestr"))
                    {
                        var sentMessage = await telegramBotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: $"Приложите файл со штампом регистрации с расширением [.xml].",
                            cancellationToken: cancellationToken,
                            replyMarkup: new ReplyKeyboardRemove());
                        this.state = RegisterDialogState.RegisterWithRosreestrDduStampStart;
                    }
                    else
                    {
                        var sentMessage = await telegramBotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: $"Данный способ регистрации временно не поддерживается",
                            cancellationToken: cancellationToken,
                            replyMarkup: new ReplyKeyboardRemove());
                        this.state = RegisterDialogState.Initial;
                    }
                    return;
                }
                case RegisterDialogState.RegisterWithRosreestrDduStampStart:
                {
                    var message = update.Message;
                    var chatId = message.Chat.Id;

                    // игнорируем обработку любых сообщений, кроме вложенных файлов
                    if (update.Message?.Document is not { } document)
                    {
                        await telegramBotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: $"Приложите файл со штампом регистрации с расширением [.xml].",
                            cancellationToken: cancellationToken);

                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : not a file message");
                        return;
                    }
                    var documentSize = message.Document.FileSize;
                    if (documentSize > 100000)
                    {
                        Message error = await telegramBotClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: $"Слишком большой файл",
                           cancellationToken: cancellationToken);
                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : too big file [{documentSize}]");
                        return;
                    }

                    var fileId = message.Document.FileId;
                    var fileInfo = await telegramBotClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;

                    // Создаём поток для чтения в память
                    using var stream = new MemoryStream();

                    // Зачитываем файл в память
                    await telegramBotClient.DownloadFileAsync(
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
                        Message error = await telegramBotClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: $"Некооректный документ",
                           cancellationToken: cancellationToken);
                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid doc, [{exception}]");
                        return;
                    }


                    // В идеале лучше разобрать Xml файл и выкусить узел registration_number, но на текущем этапе просто ищем подстроку
                    // в текстовом представлении файла
                    var fileString = Encoding.UTF8.GetString(fileContenst);
                    if (!fileString.Contains("77:05:0008007:14033-77/", StringComparison.OrdinalIgnoreCase))
                    {
                        Message error = await telegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Некооректный документ",
                            cancellationToken: cancellationToken);
                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : doc not containing expected id");
                        return;
                    }

                    await telegramBotClient.SendTextMessageAsync(
                           chatId: update.Message.Chat.Id,
                           text: $"Ожидайте проверки файла.",
                           cancellationToken: cancellationToken);

                    // Создаём запрос на проверку подписи
                    var doc = new SignedDocument()
                    {
                        SignatureType = SignatureType.XMLDSig,
                        Content = fileContenst
                    };

                    // Сериализуем в json
                    var serializedDoc = JsonConvert.SerializeObject(doc);

                    // Выставляем тип содержимого и отправляем запрос
                    var content = new StringContent(serializedDoc, Encoding.UTF8, "application/json");
                    var result = await this.svsCLient.PostAsync($"https://dss.cryptopro.ru/Verify/rest/api/signatures", content);

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
                            Message error = await telegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Не удалось распознать резульатат проверки",
                                cancellationToken: cancellationToken);
                            this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid signature validation result");
                            return;
                        }

                        verificationResult = verificationResults[0];
                    }


                    catch (Exception)
                    {
                        Message error = await telegramBotClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: $"Не удалось распознать резульатат проверки",
                               cancellationToken: cancellationToken);
                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid signature validation result");
                        return;
                    }

                    // Выкусываем подписанта, проверяем что в его имени есть росреестр
                    // NB - да, возможная атака это создание своего самоподписанного сертификата, где в имени будет росреестр.
                    // но предположим, что это за гранью возможностей рекламщиков ремонтов.
                    if (verificationResult.SignerCertificateInfo == null ||
                        !verificationResult.SignerCertificateInfo[CertificateInfoParams.SubjectName].Contains("росреестр", StringComparison.OrdinalIgnoreCase))
                    {
                        Message error = await telegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Не удалось распознать подписанта",
                            cancellationToken: cancellationToken);
                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid signature subject " +
                            $"[{verificationResult?.SignerCertificateInfo[CertificateInfoParams.SubjectName]}]");
                        return;
                    }
                    if (!verificationResult.Message.Contains("Подпись математически корректна", StringComparison.OrdinalIgnoreCase) &&
                        !verificationResult.Result)
                    {
                        Message error = await telegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Подпись некорректна",
                            cancellationToken: cancellationToken);
                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid signature");
                        return;
                    }

                    await this.userManager.AddUserAsync(
                        new()
                        {
                            TelegramId = update.Message.From.Id,
                            Role = UserEntity.UserRols.User,
                            Status = UserEntity.StatusFormats.Active
                        });

                    await telegramBotClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Регистрация пройдена!",
                        cancellationToken: cancellationToken);

                    this.dialogOrcestrator.SwitchToDialog<WellcomeDialog>(
                        update.Message.From.Id.ToString(),
                        telegramBotClient,
                        update,
                        cancellationToken,
                        true);

                    return;
                }
                default:
                {
                    throw new Exception();
                }
            }
        }

        public override Task ResetState()
        {
            this.state = RegisterDialogState.Initial;
            return Task.CompletedTask;
        }
    }

    internal enum RegisterDialogState
    {
        Initial = 0,
        SelectRegistrationType,

        //RegisterWithRosreestrDduStampInitial,
        RegisterWithRosreestrDduStampStart
    }

    internal enum AuthenticationType
    {
        RosreestrDduStamp = 1,
        Egnr = 2,
        ManualAnyDockement,
    }
}