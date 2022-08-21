using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    /// <summary>
    /// Базовый класс для диалога с пользователем
    /// </summary>
    internal abstract class DialogBase
    {

        /// <summary>
        /// Последнее время взаимодействия с диалогом
        /// </summary>
        public DateTimeOffset LastAccessTime { get; protected set; }

        /// <summary>
        /// Создаёт диалог
        /// </summary>
        /// <param name="dialogData">Данные диалога</param>
        public DialogBase()
        {
            this.LastAccessTime = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Обработка сообщения диалогом
        /// </summary>
        /// <param name="update">Сообщение для обработки</param>
        /// <param name="cnsToke">Токен отмены</param>
        public abstract Task ProcessUpdate(ITelegramBotClient telegramBotClient, Update update, CancellationToken cnsToke);
    }
}
