using Newtonsoft.Json;

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

        /// <summary>
        /// Идентификатор закрытого чата, в который бот будет присылать инвайты.
        /// Бот должен иметь права адмна в чате
        /// </summary>
        public long PrivateChatId { get; set; }

        /// <summary>
        /// Строка подключения к бд
        /// </summary>
        public string MySqlConnectionString { get; set; }

        //public BotConfiguration()
        //{
        //}

        ///// <summary>
        ///// Зачитывает конфиг из локального json файла
        ///// </summary>
        ///// <returns></returns>
        //public static async Task<BotConfiguration> GetConfigAsync()
        //{
        //    //var options = new JsonSerializerOptions()
        //    //{
        //    //    NumberHandling = JsonNumberHandling.AllowReadingFromString |
        //    //    JsonNumberHandling.WriteAsString
        //    //};

        //    return JsonConvert.DeserializeObject<BotConfiguration>(
        //        await File.ReadAllTextAsync(
        //            Path.Combine("Configuration", "config.json")));
        //}
    }
}