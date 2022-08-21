using GreenVerticalBot.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

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

        protected ILogger<DialogBase> logger;

        /// <summary>
        /// Создаёт диалог
        /// </summary>
        /// <param name="dialogData">Данные диалога</param>
        public DialogBase(
            DialogOrcestrator dialogOrcestrator,
            AppConfig config,
            ILogger<DialogBase> logger)
        {
            this.dialogOrcestrator = dialogOrcestrator;
            this.Config = config;
            this.logger = logger;
        }

        /// <summary>
        /// Обработка сообщения диалогом
        /// </summary>
        /// <param name="update">Сообщение для обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public virtual async Task ProcessUpdate(ITelegramBotClient telegramBotClient, Update update, CancellationToken cancellationToken)
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
            if (chatId == this.Config.PrivateChatId)
            {
                return;
            }

            if (update.Message?.Text == "/register")
            {
                this.dialogOrcestrator.SwitchToDialog<RegisterDialog>
                    ($"{update.Message.From.Id}",
                    telegramBotClient,
                    update,
                    cancellationToken,
                    true);
                return;
            }
            else if (update.Message?.Text == "/user")
            {
                this.dialogOrcestrator.SwitchToDialog<UserInfoDialog>
                    ($"{update.Message.From.Id}",
                    telegramBotClient,
                    update,
                    cancellationToken,
                    true);
                return;
            }
            else if (update.Message?.Text == "/authorize")
            {
                this.dialogOrcestrator.SwitchToDialog<AuthorizeDialog>
                    ($"{update.Message.From.Id}",
                    telegramBotClient,
                    update,
                    cancellationToken,
                    true);
                return;
            }

            await this.ProcessUpdateCore(telegramBotClient, update, cancellationToken);
        }

        public abstract Task ProcessUpdateCore(ITelegramBotClient telegramBotClient, Update update, CancellationToken cancellationToken);

        public abstract Task ResetState();
    }
}
