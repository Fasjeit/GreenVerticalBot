using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GreenVerticalBot.Configuration
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

        /// <summary>
        /// Строка подключения к бд
        /// </summary>
        public string MySqlConnectionString { get; set; }

        public AppConfig()
        {
        }

        /// <summary>
        /// Зачитывает конфиг из локального json файла
        /// </summary>
        /// <returns></returns>
        public static async Task<AppConfig> GetConfigAsync()
        {
            //var options = new JsonSerializerOptions()
            //{
            //    NumberHandling = JsonNumberHandling.AllowReadingFromString |
            //    JsonNumberHandling.WriteAsString
            //};

            return JsonConvert.DeserializeObject<AppConfig>(
                await File.ReadAllTextAsync(
                    Path.Combine("Configuration", "config.json")));
        }
    }
}
