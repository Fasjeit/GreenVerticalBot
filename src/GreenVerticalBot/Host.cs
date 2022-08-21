using GreenVerticalBot.EntityFramework;
using GreenVerticalBot.EntityFramework.Store;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.EntityFrameworkCore;
using GreenVerticalBot.Bot;
using GreenVerticalBot.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using GreenVerticalBot.Dialogs;

namespace GreenVerticalBot
{
    internal class GreenVerticalBotHost
    {
        public static async Task RunHost()
        {
            var builder = Host.CreateDefaultBuilder();
            var config = await AppConfig.GetConfigAsync();

            builder.ConfigureServices(
                (_, services) =>
            {
                services.AddDbContext<GreenVerticalBotContext>(
                    (serviceProvider, options) =>
                {
                    options.UseMySQL(config.MySqlConnectionString);
                });
                services.AddScoped<ITasksStore, TasksStore>();
                services.AddScoped<GreenBot>();
                services.AddHttpClient();

                services.AddSingleton<AppConfig>(config);
                services.AddSingleton<DialogOrcestrator>();

                services.AddScoped<WellcomeDialog>();
            });

            using (var host = builder.Build())
            {
                await MainRutine(host.Services, "Scope 2");

                await host.RunAsync();
            }
        }

        static async Task MainRutine(IServiceProvider services, string scope)
        {
            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            //OperationLogger logger = provider.GetRequiredService<OperationLogger>();
            //logger.LogOperations($"{scope}-Call 1 .GetRequiredService<OperationLogger>()");

            //Console.WriteLine("...");

            //logger = provider.GetRequiredService<OperationLogger>();
            //logger.LogOperations($"{scope}-Call 2 .GetRequiredService<OperationLogger>()");

            //Console.WriteLine();

            var bot = provider.GetRequiredService<GreenBot>();
            await bot.MainRutine();
        }
    }
}
