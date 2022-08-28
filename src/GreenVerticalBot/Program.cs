using Serilog;
using Serilog.Events;

namespace GreenVerticalBot
{
    internal class Program
    {
        /// <summary>
        /// Клиент для общения с сервисом проверки подписи
        /// </summary>
        private static readonly HttpClient SvsCLient = new HttpClient();

        private static async Task Main(string[] args)
        {
            //var configuration = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json", optional: false)
            //    //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            //    .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{SourceContext}] [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.File("bot_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            await GreenVerticalBotHost.RunHost();
            return;
        }
    }
}