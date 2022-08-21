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
using GreenVerticalBot.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog;

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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{SourceContext}] [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();



            await GreenVerticalBotHost.RunHost();
            return;
        }
    }
}
