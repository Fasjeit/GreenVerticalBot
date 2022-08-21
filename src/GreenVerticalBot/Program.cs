using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using GreenVerticalBot.RestModels;
using GreenVerticalBot.Logging;
using GreenVerticalBot.Configuration;
using Microsoft.Extensions.Configuration;

namespace GreenVerticalBot
{
    class Program
    {
        /// <summary>
        /// Клиент для общения с сервисом проверки подписи
        /// </summary>
        private static readonly HttpClient SvsCLient = new HttpClient();

        static async Task Main(string[] args)
        {
            //IConfiguration config = new ConfigurationBuilder()
            //    .AddJsonFile("config.json")
            //    .AddEnvironmentVariables()
            //    .Build();


            await GreenVerticalBotHost.RunHost();
            return;            
        }
    }
}
