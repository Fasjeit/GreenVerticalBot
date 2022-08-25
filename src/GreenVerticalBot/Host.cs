using GreenVerticalBot.Bot;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Dialogs;
using GreenVerticalBot.EntityFramework;
using GreenVerticalBot.EntityFramework.Store.Tasks;
using GreenVerticalBot.EntityFramework.Store.User;
using GreenVerticalBot.Monitoring;
using GreenVerticalBot.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Configuration;

namespace GreenVerticalBot
{
    internal static class GreenVerticalBotHost
    {
        public static async Task RunHost()
        {
            var builder = Host.CreateDefaultBuilder()
                .UseSerilog();

            //var config = await BotConfiguration.GetConfigAsync();

            builder.ConfigureServices(
                (context, services) =>
            {
                // Setup Bot configuration
                services.Configure<BotConfiguration>(
                    context.Configuration.GetSection(nameof(BotConfiguration)));

                services.AddDbContext<GreenVerticalBotContext>(
                    (IServiceProvider serviceProvider, DbContextOptionsBuilder options) =>
                {
                    var configuration = serviceProvider.GetConfiguration<BotConfiguration>();
                    options.UseMySQL(configuration.MySqlConnectionString);
                });

                services.AddScoped<ITaskStore, TaskStore>();
                services.AddScoped<EntityFramework.Store.User.IUserStore, UserStore>();

                services.AddScoped(
                    (services) => new DialogData());

                services.AddScoped<GreenBot>();
                services.AddHttpClient();

                services.AddSingleton<BotConfiguration>(
                    (IServiceProvider serviceProvider) =>
                {
                        return serviceProvider.GetConfiguration<BotConfiguration>();
                });
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

        private static async Task MainRutine(IServiceProvider services, string scope)
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