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
using GreenVerticalBot.Logs;
using GreenVerticalBot.Configuration;

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
            await GreenVerticalBotHost.RunHost();
            return;            
        }
    }
}
