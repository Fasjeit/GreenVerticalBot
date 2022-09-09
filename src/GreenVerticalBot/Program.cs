using GreenVerticalBot.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace GreenVerticalBot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
            //.MinimumLevel.Verbose()
            //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{SourceContext}] [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.File("bot_log.txt", rollingInterval: RollingInterval.Day, shared: true)
                .WriteTo.Sink(new InMemorySink())
                .CreateLogger();

            await GreenVerticalBotHost.RunHost();
            return;
        }
    }
}