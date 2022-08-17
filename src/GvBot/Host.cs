using GvBot.EntityFramework;
using GvBot.EntityFramework.Store;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.EntityFrameworkCore;
using GvBot.Bot;
using GvBot.Configuration;

namespace GvBot
{
    internal class GvBotHost
    {
        public static async Task RunHost()
        {
            var builder = Host.CreateDefaultBuilder();
            var config = await AppConfig.GetConfigAsync();

            builder.ConfigureServices(
                (_, services) =>
            {
                services.AddDbContext<GvBotContext>(
                    (serviceProvider, options) =>
                {
                    options.UseMySQL(config.MySqlConnectionString);
                });
                services.AddScoped<ITasksStore, TasksStore>();
                services.AddScoped<VerificationBot>();
                services.AddHttpClient();
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
            var bot = provider.GetRequiredService<VerificationBot>();
            await bot.MainRutine();
        }
    }
}
