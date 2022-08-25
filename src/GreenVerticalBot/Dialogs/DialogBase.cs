using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Telegram.Bot;
using Update = Telegram.Bot.Types.Update;

namespace GreenVerticalBot.Dialogs
{
    /// <summary>
    /// Базовый класс для диалога с пользователем
    /// </summary>
    internal abstract class DialogBase
    {
        protected readonly DialogOrcestrator dialogOrcestrator;

        /// <summary>
        /// Конфигурация бота
        /// </summary>
        protected AppConfig Config { get; set; }

        protected ILogger<DialogBase> Logger { get; set; }

        private IUserManager userManager { get; set; }

        protected DialogData Data { get; set;}

        /// <summary>
        /// Создаёт диалог
        /// </summary>
        /// <param name="dialogData">Данные диалога</param>
        public DialogBase(
            DialogOrcestrator dialogOrcestrator,
            AppConfig config,
            IUserManager userManager,
            DialogData data,
            ILogger<DialogBase> logger)
        {
            this.dialogOrcestrator = dialogOrcestrator
                ?? throw new ArgumentNullException(nameof(dialogOrcestrator));
            this.Config = config
                ?? throw new ArgumentNullException(nameof(config));
            this.Logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this.userManager = userManager
                ?? throw new ArgumentNullException(nameof(userManager));
            this.Data = data
                ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Обработка сообщения диалогом
        /// </summary>
        /// <param name="update">Сообщение для обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public virtual async Task ProcessUpdateAsync(ITelegramBotClient telegramBotClient, Update update, CancellationToken cancellationToken)
        {
                await this.SetDialogDataAsync(
                    telegramBotClient,
                    update,
                    cancellationToken);

                var user = this.Data.User;
                if (user != null &&
                    user.LastAccessTime < DateTime.UtcNow - TimeSpan.FromMinutes(1))
                {
                    // Если пользователь был активен более бинуты назад - обновляем время активности
                    user.LastAccessTime = DateTimeOffset.UtcNow;
                }

                // Не обрабатываем сообщений, которые пришли ранее, чем минуту назад
                if (update?.Message?.Date < DateTime.UtcNow - TimeSpan.FromMinutes(1))
                {
                    return;
                }

                if (update?.CallbackQuery?.Message?.Date < DateTime.UtcNow - TimeSpan.FromMinutes(1))
                {
                    return;
                }

                // Проверка авторизации
                bool isAuthorized = true;
                object[] attrs = this.GetType().GetCustomAttributes(true);
                var rollesAttribute = attrs.FirstOrDefault(a => a is AuthorizeRolesAttribute);
                if (rollesAttribute != null)
                {
                    if (this.Data?.Claims == null)
                    {
                        isAuthorized = false;
                    }
                    else
                    {
                        isAuthorized = ((AuthorizeRolesAttribute)rollesAttribute).RoleArray
                            .Any(
                                r => this.Data.Claims.Any(
                                    dc => dc.Value == r.ToString()));
                    }
                }
                if (!isAuthorized)
                {
                    this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                            $": unauthorized");

                    await telegramBotClient.SendTextMessageAsync(
                        chatId: this.Data.ChatId,
                        text:
                            $"Доступ запрещён.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);

                    await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                        this.Data.ChatId.ToString(),
                        telegramBotClient,
                        update,
                        cancellationToken,
                        true);
                }

                if (update?.Message?.Text == "/register")
                {
                    await this.dialogOrcestrator.SwitchToDialogAsync<RegisterDialog>
                        ($"{this.Data.ChatId}",
                        telegramBotClient,
                        update,
                        cancellationToken,
                        true);
                    return;
                }
                else if (update?.Message?.Text == "/user")
                {
                    await this.dialogOrcestrator.SwitchToDialogAsync<UserInfoDialog>
                        ($"{this.Data.ChatId}",
                        telegramBotClient,
                        update,
                        cancellationToken,
                        true);
                    return;
                }
                else if (update?.Message?.Text == "/authorize")
                {
                    await this.dialogOrcestrator.SwitchToDialogAsync<AuthorizeDialog>
                        ($"{this.Data.ChatId}",
                        telegramBotClient,
                        update,
                        cancellationToken,
                        true);
                    return;
                }

                else if (update?.Message?.Text == "/help")
                {
                    await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>
                        ($"{this.Data.ChatId}",
                        telegramBotClient,
                        update,
                        cancellationToken,
                        true);
                    return;
                }

                //var chatId = message.Chat.Id;

                //// Игнорируем сообщения в целевом чате (в него только выдаём инвайты)
                //if (chatId == this.Config.PrivateChatId)
                //{
                //    return;
                //}            

                await this.ProcessUpdateCoreAsync(telegramBotClient, update, cancellationToken);

        }

        internal abstract Task ProcessUpdateCoreAsync(ITelegramBotClient telegramBotClient, Update update, CancellationToken cancellationToken);

        public abstract Task ResetStateAsync();

        protected async Task SetDialogDataAsync(
            ITelegramBotClient telegramBotClient,
            Update update,
            CancellationToken cancellationToken
            )
        {
            //if (this.Data != null)
            //{
            //    return;
            //}

            var userId = DialogBase.GetUserId(update);
            var user = await this.userManager.GetUserByTelegramIdAsync(userId);
            if (user != null)
            {
                user.LastAccessTime = DateTime.UtcNow;
                await this.userManager.UpdateUserAsync(user);
            }

            this.Data.BotClient = telegramBotClient;
            this.Data.Update = update;
            this.Data.User = user;
            this.Data.TelegramUserId = userId;
            this.Data.CancellationToken = cancellationToken;
            this.Data.Claims = user?.Claims;
        }

        public static long GetUserId(Update update)
        {
            var userId =
                update?.Message?.From?.Id ??
                update?.CallbackQuery?.From?.Id;
            if (userId == null)
            {
                throw new ArgumentOutOfRangeException(nameof(update));
            }
            return (long)userId;
        }
    }
}
