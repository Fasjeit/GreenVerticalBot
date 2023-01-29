using GreenVerticalBot.Bot;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Dialogs;
using GreenVerticalBot.EntityFramework;
using GreenVerticalBot.EntityFramework.Store.Tasks;
using GreenVerticalBot.EntityFramework.Store.Users;
using GreenVerticalBot.Logging;
using GreenVerticalBot.Monitoring;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GreenVerticalBot
{
    internal static class GreenVerticalBotHost
    {
        public static async Task RunHost()
        {
            var builder = Host.CreateDefaultBuilder();

            builder.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{SourceContext}] [{Level}] {Message}{NewLine}{Exception}")
                    .WriteTo.File("bot_log.txt", rollingInterval: RollingInterval.Day, shared: true)
                    .WriteTo.Sink(new InMemorySink());
            });

            builder.ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.Sources.Clear();

                configuration.SetBasePath(Directory.GetCurrentDirectory())
#if DEV
                .AddJsonFile("appsettings.Development.json", optional: false)
#else
                .AddJsonFile("appsettings.json", optional: false)
#endif
                //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .Build();
            });

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
                    options.UseMySql(
                        configuration.MySqlConnectionString,
                        MariaDbServerVersion.LatestSupportedServerVersion);
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
                services.AddScoped<TasksInfoDialog>();
                services.AddScoped<QrDialog>();
                services.AddScoped<ApproveDialog>();
                services.AddScoped<LogsDialog>();

                services.AddScoped<IUserManager, UserManager>();
                services.AddScoped<ITaskManager, TaskManager>();

                services.AddHostedService<ScopeMonitor>();
            });

            using (var host = builder.Build())
            {
                var cts = new CancellationTokenSource();
                await MainRutine(host.Services, cts.Token);
                await host.RunAsync(cts.Token);
            }
        }

        private static async Task MainRutine(IServiceProvider services, CancellationToken cnsToken)
        {
            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var bot = provider.GetRequiredService<GreenBot>();
            await bot.MainRutine(cnsToken);
        }
    }
}