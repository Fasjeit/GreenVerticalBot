﻿using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Entities;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.RestModels;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Tasks.Data;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;
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

    internal class AuthenticateDialog : DialogBase
    {
        private RegisterDialogState state = RegisterDialogState.Initial;

        private readonly HttpClient svsCLient;

        private readonly IUserManager userManager;

        private readonly ITaskManager taskManager;

        public AuthenticateDialog(
            IUserManager userManager,
            ITaskManager taskManager,
            IHttpClientFactory httpClientFactory,
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            DialogContext data,
            ILogger<AuthenticateDialog> logger)
            : base(dialogOrcestrator, config, userManager, data, logger)
        {
            this.userManager = userManager ??
                throw new ArgumentNullException(nameof(userManager));
            this.taskManager = taskManager ??
                throw new ArgumentNullException(nameof(taskManager));

            this.svsCLient = httpClientFactory.CreateClient();
        }

        internal override async Task ProcessUpdateCoreAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            var userId = DialogBase.GetUserId(update);

            switch (this.state)
            {
                case RegisterDialogState.Initial:
                {
                    var user = await this.userManager.GetUserByTelegramIdAsync(this.Context.TelegramUserId);

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
                        OneTimeKeyboard = true,
                        ResizeKeyboard = true,
                    };

                    await botClient.SendTextMessageAsync(
                        chatId: this.Context.ChatId,
                        text:
                            $"Выберите способы регистации:{Environment.NewLine}{Environment.NewLine}" +
                            $"* /rosreestr Штамп о регистрации ДДУ в Росреестре [ Житель жк ] {Environment.NewLine}" +
                            $"* /etc Прочий документ, подтверждающий владение [ Житель жк, Житель корпуса ]",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        //replyMarkup: keyboard,
                        cancellationToken: cancellationToken);
                    this.state = RegisterDialogState.SelectRegistrationType;
                    return;
                }
                case RegisterDialogState.SelectRegistrationType:
                {
                    if (update?.Message?.Text is string{ } text && text.StartsWith("/rosreestr"))
                    {
                        if (this.Context?.User?.Claims != null &&
                            this.Context.User.Claims.Any(c => c.Value == UserRole.RegisteredUser.ToString()))
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: this.Context.ChatId,
                                text:
                                    $"Пользователь уже зарегистрирован.{Environment.NewLine}" +
                                    $"Используйте команду /user для просмотра информации",
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                                cancellationToken: cancellationToken);

                            await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                                this.Context.TelegramUserId.ToString(),
                                 botClient,
                                 update,
                                 cancellationToken);
                            return;
                        }

                        await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"Приложите файл со штампом регистрации с расширением [.xml].",
                            cancellationToken: cancellationToken,
                            replyMarkup: new ReplyKeyboardRemove());
                        this.state = RegisterDialogState.RegisterWithRosreestrDduStampStart;
                    }
                    else if (update!.Message!.Text!.StartsWith("/etc"))
                    {
                        this.state = RegisterDialogState.EtcSelectClaimType;
                        // step into state
                        await this.ProcessUpdateCoreAsync(botClient, update, cancellationToken);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: userId,
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

                    // игнорируем обработку любых сообщений, кроме вложенных файлов
                    if (update.Message?.Document is not { } document)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"Приложите файл со штампом регистрации с расширением [.xml].",
                            cancellationToken: cancellationToken);

                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : not a file message");
                        return;
                    }
                    var documentSize = message.Document.FileSize;
                    if (documentSize > 100000)
                    {
                        Message error = await botClient.SendTextMessageAsync(
                           chatId: this.Context.ChatId,
                           text: $"Слишком большой файл",
                           cancellationToken: cancellationToken);
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : too big file [{documentSize}]");
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
                           chatId: this.Context.ChatId,
                           text: $"Некооректный документ",
                           cancellationToken: cancellationToken);
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}]: invalid doc, [{exception}]");
                        return;
                    }

                    // В идеале лучше разобрать Xml файл и выкусить узел registration_number, но на текущем этапе просто ищем подстроку
                    // в текстовом представлении файла
                    var fileString = Encoding.UTF8.GetString(fileContenst);
                    if (!fileString.Contains("77:05:0008007:14033-77/", StringComparison.OrdinalIgnoreCase))
                    {
                        Message error = await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"Некооректный документ",
                            cancellationToken: cancellationToken);
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : doc not containing expected id");
                        return;
                    }

                    await botClient.SendTextMessageAsync(
                           chatId: this.Context.ChatId,
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
                            Message error = await botClient.SendTextMessageAsync(
                                chatId: this.Context.ChatId,
                                text: $"Не удалось распознать резульатат проверки",
                                cancellationToken: cancellationToken);
                            this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid signature validation result");
                            return;
                        }

                        verificationResult = verificationResults[0];
                    }
                    catch (Exception)
                    {
                        Message error = await botClient.SendTextMessageAsync(
                               chatId: this.Context.ChatId,
                               text: $"Не удалось распознать резульатат проверки",
                               cancellationToken: cancellationToken);
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid signature validation result");
                        return;
                    }

                    // Выкусываем подписанта, проверяем что в его имени есть росреестр
                    // NB - да, возможная атака это создание своего самоподписанного сертификата, где в имени будет росреестр.
                    // но предположим, что это за гранью возможностей рекламщиков ремонтов.
                    if (verificationResult.SignerCertificateInfo == null ||
                        !verificationResult.SignerCertificateInfo[CertificateInfoParams.SubjectName].Contains("росреестр", StringComparison.OrdinalIgnoreCase))
                    {
                        Message error = await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"Не удалось распознать подписанта",
                            cancellationToken: cancellationToken);
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid signature subject " +
                            $"[{verificationResult?.SignerCertificateInfo[CertificateInfoParams.SubjectName]}]");
                        return;
                    }
                    if (!verificationResult.Message.Contains("Подпись математически корректна", StringComparison.OrdinalIgnoreCase) &&
                        !verificationResult.Result)
                    {
                        Message error = await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"Подпись некорректна",
                            cancellationToken: cancellationToken);
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : invalid signature");
                        return;
                    }

                    var claim = new BotClaim(
                        type: ClaimTypes.Role,
                        value: UserRole.RegisteredUser.ToString(),
                        valueType: null,
                        issuer: "green_bot",
                        originalIssuer: null);

                    var claims = new List<BotClaim>() { claim };

                    // Создаём запись об успешной задаче выдаче утверждения
                    var task = new BotTask()
                    {
                        Status = StatusFormats.Approved,
                        LinkedObject = this.Context.TelegramUserId.ToString(),
                        Type = TaskType.RequestClaim,
                        Data = new RequestClaimTaskData() { Claims = claims, Reason = "Подтверждено ботом" },
                    };
                    await this.taskManager.AddTaskAsync(task);

                    await this.userManager.AddUserAsync(
                        new()
                        {
                            TelegramId = userId,
                            Claims = claims,
                            Status = UserEntity.StatusFormats.Active
                        });

                    await botClient.SendTextMessageAsync(
                        chatId: this.Context.ChatId,
                        text: $"Регистрация пройдена!",
                        cancellationToken: cancellationToken);

                    await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                        userId.ToString(),
                        botClient,
                        update,
                        cancellationToken,
                        true);

                    return;
                }
                case RegisterDialogState.EtcSelectClaimType:
                {
                    var userTasks = await this.taskManager.GetTasksByLinkedObjectAsync(
                        this.Context.TelegramUserId.ToString());
                    if (userTasks.Any(t =>
                        t.Status == StatusFormats.Created &&
                        t.Type == TaskType.RequestClaim))
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: userId,
                            text: $"У вас уже есть активный запрос. {Environment.NewLine}" +
                            $" Для провреки его статуса используте команду /tasks",
                            cancellationToken: cancellationToken,
                            replyMarkup: new ReplyKeyboardRemove());
                        this.state = RegisterDialogState.Initial;
                        return;
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine("Выберите утрверждеие для получения:");
                    sb.AppendLine("/k9 - 🏡 житель 9 корпуса (10 строительный)");

                    await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: sb.ToString(),
                            cancellationToken: cancellationToken);
                    this.state = RegisterDialogState.EtcUploadFilePromt;
                    return;
                }
                case RegisterDialogState.EtcUploadFilePromt:
                {

                    // parce and rememder requredCLaims
                    if (update?.Message?.Text != null &&
                        update.Message.Text.StartsWith("/k9"))
                    {
                        this.Context.ContextDataString["required_claim"] = UserRole.AccessToB9_ex10Chat.ToString();
                    }
                    else
                    {
                        return;
                    }

                    await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"Приложите файл (дду, выписка из ЕГРН, прочий документ на собственность).",
                            cancellationToken: cancellationToken);

                    this.state = RegisterDialogState.EtcLoadFile;
                    return;
                }
                case RegisterDialogState.EtcLoadFile:
                {
                    var message = update.Message;

                    // игнорируем обработку любых сообщений, кроме вложенных файлов
                    if (update.Message?.Document is not { } document)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"Приложите файл (дду, выписка из ЕГРН, прочий документ на собственность).",
                            cancellationToken: cancellationToken);

                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : not a file message");
                        return;
                    }
                    var documentSize = message.Document.FileSize;
                    if (documentSize > 100000)
                    {
                        Message error = await botClient.SendTextMessageAsync(
                           chatId: this.Context.ChatId,
                           text: $"Слишком большой файл",
                           cancellationToken: cancellationToken);
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : too big file [{documentSize}]");
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
                    stream.Position = 0;

                    // Создаём задачу на подтверждение
                    var task = new BotTask()
                    {
                        Status = StatusFormats.Created,
                        Type = TaskType.RequestClaim,
                        LinkedObject = this.Context.TelegramUserId.ToString(),
                        Data = new RequestClaimTaskData()
                        {
                            Claims = new List<BotClaim>()
                            {
                                new BotClaim(
                                     type: ClaimTypes.Role,
                                     value: UserRole.RegisteredUser.ToString(),
                                     valueType: null,
                                     issuer: "operator",
                                     originalIssuer: null),
                                new BotClaim(
                                     type: ClaimTypes.Role,
                                     value: this.Context.ContextDataString["required_claim"],
                                     valueType: null,
                                     issuer: "operator",
                                     originalIssuer: null),
                            },
                            FileProofBase64 = Convert.ToBase64String(stream.ToArray()),
                            FileProofName = message.Document.FileName,
                            ShouldBeApprovedByAny = new List<UserRole>()
                            {
                                UserRole.OperatorAccessToB9_ex10Chat
                            }
                        }
                    };
                    await this.taskManager.AddTaskAsync(task);

                    await botClient.SendTextMessageAsync(
                           chatId: userId,
                           text: $"Запрос зарегистрирован.{Environment.NewLine}" +
                           $"За стутсусом запроса можно следить через команду /tasks",
                           cancellationToken: cancellationToken,
                           replyMarkup: new ReplyKeyboardRemove());

                    await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                        this.Context.ChatId.ToString(), 
                        botClient, 
                        update, 
                        cancellationToken, 
                        true);
                    return;
                }
                default:
                {
                    await botClient.SendTextMessageAsync(
                            chatId: userId,
                            text: $"Данный способ регистрации временно не поддерживается",
                            cancellationToken: cancellationToken,
                            replyMarkup: new ReplyKeyboardRemove());
                    this.state = RegisterDialogState.Initial;
                    return;
                }
            }
        }

        public override Task ResetStateAsync()
        {
            this.state = RegisterDialogState.Initial;
            return Task.CompletedTask;
        }
    }

    internal enum RegisterDialogState
    {
        Initial = 0,
        SelectRegistrationType,
        RegisterWithRosreestrDduStampStart,

        EtcSelectClaimType,
        EtcUploadFilePromt,
        EtcLoadFile,
    }

    internal enum AuthenticationType
    {
        RosreestrDduStamp = 1,
        Egnr = 2,
        ManualAnyDockement,
    }
}