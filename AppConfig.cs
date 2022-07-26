using Newtonsoft.Json;

namespace GvBot
{
    /// <summary>
    /// Конфигурация бота
    /// </summary>
    internal class AppConfig
    {
        /// <summary>
        /// Токен бота (получаем через BotFather телеграма)
        /// </summary>
        public string BotToken { get; set; }

        /// <summary>
        /// Идентификатор закрытого чата, в который бот будет присылать инвайты. 
        /// Бот должен иметь права адмна в чате
        /// </summary>
        public long PrivateChatId { get; set; }

        private AppConfig()
        {
        }

        /// <summary>
        /// Зачитывает конфиг из локального json файла
        /// </summary>
        /// <returns></returns>
        public static async Task<AppConfig> GetConfigAsync()
        {
            return JsonConvert.DeserializeObject<AppConfig>(await File.ReadAllTextAsync(@"config.json"));
        }
    }
}
