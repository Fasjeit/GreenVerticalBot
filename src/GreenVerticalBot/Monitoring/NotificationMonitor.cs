//using GreenVerticalBot.Authorization;
//using GreenVerticalBot.Configuration;
//using GreenVerticalBot.Dialogs;
//using GreenVerticalBot.EntityFramework.Store.User;
//using GreenVerticalBot.Tasks;
//using GreenVerticalBot.Users;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Telegram.Bot;

//namespace GreenVerticalBot.Monitoring
//{
//    internal class NotificationMonitor : IHostedService, IDisposable
//    {
//        private readonly ITaskManager taskManager;
//        private readonly IUserManager userManager;
//        private readonly BotConfiguration botConfiguration;
//        private readonly ILogger<ScopeMonitor> logger;
//        private readonly ITelegramBotClient botClient;


//        /// <summary>
//        /// Флаг, определяющий, что слушатель запущен.
//        /// </summary>
//        private bool started;

//        /// <summary>
//        /// Мониторинг производится в настоящий момент
//        /// </summary>
//        private bool isRunning = false;

//        /// <summary>
//        /// Получает или задает таймер мониторинга истёкших транзакций.
//        /// </summary>
//        private System.Timers.Timer expirationMonitorTimer;

//        private bool disposedValue;

//        /// <summary>
//        /// Gets the avaible services.
//        /// </summary>
//        public IServiceProvider ServiceProvider { get; }

//        public NotificationMonitor(
//            IServiceProvider serviceProvider)
//        {
//            this.botConfiguration = serviceProvider.GetRequiredService<BotConfiguration>()
//               ?? throw new ArgumentNullException(nameof(botConfiguration));
//            this.userManager = serviceProvider.GetRequiredService<IUserManager>()
//               ?? throw new ArgumentNullException(nameof(userManager));
//            this.taskManager = serviceProvider.GetRequiredService<ITaskManager>()
//                ?? throw new ArgumentNullException(nameof(taskManager));
//            this.botClient = serviceProvider.GetRequiredService<ITelegramBotClient>()
//                ?? throw new ArgumentNullException(nameof(botClient));
//            this.logger = serviceProvider.GetRequiredService<ILogger<ScopeMonitor>>()
//                ?? throw new ArgumentNullException(nameof(logger));
//        }

//        public Task StartAsync(CancellationToken cancellationToken)
//        {
//            //this.logger?.MonitoringStartPending();
//            if (this.started)
//            {
//                return Task.CompletedTask;
//            }

//            uint interval = 10 * 60; //seconds
//            //using (var scope = this.ServiceProvider.CreateScope())
//            //{
//            //    var configurationManager =
//            //        scope.ServiceProvider.GetRequiredService<IOptions<DocumentStoreConfiguration>>();

//            //    // Минуты в секунды
//            //    //interval = configurationManager.DocumentsMonitoringPeriod * 60;
//            //    interval = this.configuration.DocumentsMonitoringPeriod * 60;
//            //}

//            if (interval == 0)
//            {
//                this.started = false;
//                return Task.CompletedTask;
//            }

//            this.expirationMonitorTimer = new System.Timers.Timer
//            {
//                Interval = interval * 1000,
//                AutoReset = false,
//                Enabled = false
//            };

//            this.expirationMonitorTimer.Elapsed += async (sender, e) =>
//            {
//                try
//                {
//                    await this.ProcessAsync(cancellationToken);
//                }
//                finally
//                {
//                    ((System.Timers.Timer)sender).Enabled = true;
//                }
//            };

//            this.expirationMonitorTimer.Start();

//            this.started = true;

//            //this.logger?.MonitoringStartComplited();

//            return Task.CompletedTask;
//        }

//        public Task StopAsync(CancellationToken cancellationToken)
//        {
//            //this.logger?.MonitoringStopPending();
//            if (!this.started)
//            {
//                return Task.CompletedTask;
//            }

//            this.started = false;
//            this.expirationMonitorTimer.Stop();
//            //this.logger?.MonitoringStopComplited();

//            return Task.CompletedTask;
//        }

//        protected async Task ProcessAsync(CancellationToken stoppingToken)
//        {
//            try
//            {
//                foreach (var extraClaimUser in this.botConfiguration.ExtraClaims)
//                {
//                    var claims = extraClaimUser.Value.Select(c => Enum.Parse<UserRole>(c)).ToArray();
//                    var tasksToApprove = await this.taskManager.GetTasksToApproveByRequredClaimAsync(claims);
//                    if (tasksToApprove.Length != 0)
//                    {
//                    }
//                }


//                // #Q_ tmp filter
//                // only Request claim
//                tasksToApprove = tasksToApprove
//                    .Where(
//                        tta => tta.Type == EntityFramework.Entities.Tasks.TaskType.RequestClaim)
//                    .ToArray();

//                if (tasksToApprove.Length == 0)
//                {
//                }
//            catch (Exception ex)
//            {
//                this.logger.LogCritical($"Error during notification: [{ex.ToString()}]");
//            }
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    this.expirationMonitorTimer.Dispose();
//                }

//                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
//                // TODO: set large fields to null
//                disposedValue = true;
//            }
//        }

//        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
//        // ~ScopeMonitor()
//        // {
//        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//        //     Dispose(disposing: false);
//        // }

//        public void Dispose()
//        {
//            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}