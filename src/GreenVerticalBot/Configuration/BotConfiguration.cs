using GreenVerticalBot.Authorization;

namespace GreenVerticalBot.Configuration
{
    /// <summary>
    /// Конфигурация бота
    /// </summary>
    internal class BotConfiguration
    {
        /// <summary>
        /// Токен бота (получаем через BotFather телеграма)
        /// </summary>
        public string BotToken { get; set; }

        ///// <summary>
        ///// Идентификатор закрытого чата, в который бот будет присылать инвайты.
        ///// Бот должен иметь права адмна в чате
        ///// </summary>
        //public long PrivateChatId { get; set; }

        /// <summary>
        /// Строка подключения к бд
        /// </summary>
        public string MySqlConnectionString { get; set; }

        public Dictionary<string, List<string>> ExtraClaims { get; set; }

        public Dictionary<string, ChatInfo> ChatInfos { get; set; }
    }

    public class ChatInfo
    {
        public string FriendlyName { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public string RequredClaim { get; set; } = UserRole.RegisteredUser.ToString();
    }
}