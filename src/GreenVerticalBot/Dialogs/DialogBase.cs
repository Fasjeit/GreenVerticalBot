using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
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
        protected BotConfiguration Config { get; set; }

        protected ILogger<DialogBase> Logger { get; set; }

        private IUserManager userManager { get; set; }

        public DialogContext Context { get; protected set; }

        /// <summary>
        /// Создаёт диалог
        /// </summary>
        /// <param name="dialogData">Данные диалога</param>
        public DialogBase(
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            IUserManager userManager,
            DialogContext context,
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
            this.Context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Обработка сообщения диалогом
        /// </summary>
        /// <param name="update">Сообщение для обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public virtual async Task ProcessUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await this.SetDialogContextAsync(
                botClient,
                update,
                cancellationToken);

            var user = this.Context.User;
            if (user != null &&
                user.LastAccessTime < DateTime.UtcNow - TimeSpan.FromMinutes(1))
            {
                // Если пользователь был активен более бинуты назад - обновляем время активности
                user.LastAccessTime = DateTimeOffset.UtcNow;
            }

            await this.ProcessUpdateCoreAsync(botClient, update, cancellationToken);
        }

        internal abstract Task ProcessUpdateCoreAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);

        public abstract Task ResetStateAsync();

        public async Task SetDialogContextAsync(
            ITelegramBotClient botClient,
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

            this.Context.BotClient = botClient;
            this.Context.Update = update;
            this.Context.User = user;
            this.Context.TelegramUserId = userId;
            this.Context.ChatId = update?.Message?.Chat?.Id;
            this.Context.CancellationToken = cancellationToken;
            this.Context.Claims = user?.Claims ?? new List<Authorization.BotClaim>();

            // Выставляем админсткие Claims из конфига
            var userid = this.Context.TelegramUserId.ToString();
            if (this.Config?.ExtraClaims != null &&
                this.Config.ExtraClaims.Keys.Contains(userid))
            {
                foreach (var claim in this.Config.ExtraClaims[userid])
                {
                    Context.Claims.Add(
                        new BotClaim(
                            type: ClaimTypes.Role,
                            value: claim,
                            valueType: null,
                            issuer: "config",
                            originalIssuer: null));
                }
            }
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

        public static long GetDialogId(Update update)
        {
            var chatId =
                update?.Message?.Chat.Id;
            if (chatId == null)
            {
                throw new ArgumentOutOfRangeException(nameof(update));
            }
            return (long)chatId;
        }

        public static bool IsGroupMessage(Update update) => update?.Message?.Chat?.Id != update?.Message?.From?.Id;
    }
}