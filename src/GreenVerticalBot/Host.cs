using GreenVerticalBot.EntityFramework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.EntityFrameworkCore;
using GreenVerticalBot.Bot;
using GreenVerticalBot.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using GreenVerticalBot.Dialogs;
using GreenVerticalBot.EntityFramework.Store.Tasks;
using GreenVerticalBot.Users;
using GreenVerticalBot.EntityFramework.Store.User;
using Serilog;
using GreenVerticalBot.Monitoring;

namespace GreenVerticalBot
{
    internal class GreenVerticalBotHost
    {
        public static async Task RunHost()
        {
            var builder = Host.CreateDefaultBuilder()
                .UseSerilog();

            var config = await AppConfig.GetConfigAsync();

            builder.ConfigureServices(
                (_, services) =>
            {
                services.AddDbContext<GreenVerticalBotContext>(
                    (IServiceProvider serviceProvider, DbContextOptionsBuilder options) =>
                {
                    options.UseMySQL(config.MySqlConnectionString);
                });

                services.AddScoped<ITaskStore, TaskStore>();
                services.AddScoped<EntityFramework.Store.User.IUserStore, UserStore>();

                services.AddScoped(
                    (services) => new DialogData());

                services.AddScoped<GreenBot>();
                services.AddHttpClient();

                services.AddSingleton(config);
                services.AddSingleton<DialogOrcestrator>();

                services.AddScoped<WellcomeDialog>();
                services.AddScoped<RegisterDialog>();
                services.AddScoped<UserInfoDialog>();
                services.AddScoped<AuthorizeDialog>();

                services.AddScoped<Users.IUserManager, UserManager>();


                services.AddHostedService<ScopeMonitor>();
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
