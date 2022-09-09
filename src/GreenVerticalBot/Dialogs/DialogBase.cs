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

        /// <summary>
        /// Логгер
        /// </summary>
        protected ILogger<DialogBase> Logger { get; set; }

        /// <summary>
        /// Менеджер пользователей
        /// </summary>
        private IUserManager userManager { get; set; }

        /// <summary>
        /// Контекст диалога
        /// </summary>
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

            await this.ProcessUpdateCoreAsync(botClient, update, cancellationToken);
        }

        /// <summary>
        /// Обработка сообщения реализацией диалога
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal abstract Task ProcessUpdateCoreAsync(
            ITelegramBotClient botClient, 
            Update update, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Сброс состояние диалога в начальное
        /// </summary>
        /// <returns></returns>
        public abstract Task ResetStateAsync();

        /// <summary>
        /// Заполнить контекст диалога на основе текущий данных
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
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

            if (update?.Message?.Chat == null)
            {
                throw new ArgumentNullException(nameof(update.Message));
            }

            var userId = DialogBase.GetUserId(update);
            var user = await this.userManager.GetUserByTelegramIdAsync(userId);
            if (user != null &&
                user.LastAccessTime < DateTime.UtcNow - TimeSpan.FromMinutes(1))
            {
                // Если пользователь был активен более минуты назад - обновляем время активности
                user.LastAccessTime = DateTimeOffset.UtcNow;
                await this.userManager.UpdateUserAsync(user);
            }

            this.Context.BotClient = botClient;
            this.Context.Update = update;
            this.Context.User = user;
            this.Context.TelegramUserId = userId;
            this.Context.ChatId = update!.Message!.Chat.Id;
            this.Context.CancellationToken = cancellationToken;
            this.Context.Claims = user?.Claims ?? new List<BotClaim>();

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

        /// <summary>
        /// Получить идентификатор пользователя из сообщения
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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

        /// <summary>
        /// Получить идентификатор чата из сообщения
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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

        /// <summary>
        /// Является ли данное сообщение сообщением в групповом чате
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public static bool IsGroupMessage(Update update) => update?.Message?.Chat?.Id != update?.Message?.From?.Id;
    }
}