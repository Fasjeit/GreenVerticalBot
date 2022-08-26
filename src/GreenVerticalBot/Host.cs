using GreenVerticalBot.Bot;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Dialogs;
using GreenVerticalBot.EntityFramework;
using GreenVerticalBot.EntityFramework.Store.Tasks;
using GreenVerticalBot.EntityFramework.Store.User;
using GreenVerticalBot.Monitoring;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

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
                services.AddScoped<IUserStore, UserStore>();

                services.AddScoped(
                    (services) => new DialogContext());

                services.AddScoped<GreenBot>();
                services.AddHttpClient();

                services.AddSingleton<BotConfiguration>(
                    (IServiceProvider serviceProvider) =>
                {
                    return serviceProvider.GetConfiguration<BotConfiguration>();
                });
                services.AddSingleton<DialogOrcestrator>();

                services.AddScoped<WellcomeDialog>();
                services.AddScoped<AuthenticateDialog>();
                services.AddScoped<UserInfoDialog>();
                services.AddScoped<AuthorizeDialog>();
                services.AddScoped<UserLookUpDialog>();
                services.AddScoped<GroupDialog>();
                services.AddScoped<ShowStatusDialog>();

                services.AddScoped<IUserManager, UserManager>();
                services.AddScoped<ITaskManager, TaskManager>();

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

            var bot = provider.GetRequiredService<GreenBot>();
            await bot.MainRutine();
        }
    }
}