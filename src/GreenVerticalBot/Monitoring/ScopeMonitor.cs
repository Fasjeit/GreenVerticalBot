using GreenVerticalBot.Dialogs;
using GreenVerticalBot.EntityFramework.Store.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenVerticalBot.Monitoring
{
    internal class ScopeMonitor : IHostedService, IDisposable
    {
        private DialogOrcestrator dialogOrcestrator;
        private IUserStore userStore;
        private ILogger<ScopeMonitor> logger;

        /// <summary>
        /// Флаг, определяющий, что слушатель запущен.
        /// </summary>
        private bool started;

        /// <summary>
        /// Мониторинг производится в настоящий момент
        /// </summary>
        private bool isRunning = false;

        /// <summary>
        /// Получает или задает таймер мониторинга истёкших транзакций.
        /// </summary>
        private System.Timers.Timer expirationMonitorTimer;

        private bool disposedValue;

        /// <summary>
        /// Gets the avaible services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        public ScopeMonitor(
            IServiceProvider serviceProvider)
        {
            this.userStore = serviceProvider.GetRequiredService<IUserStore>()
                ?? throw new ArgumentNullException(nameof(userStore));
            this.dialogOrcestrator = serviceProvider.GetRequiredService<DialogOrcestrator>()
                ?? throw new ArgumentNullException(nameof(dialogOrcestrator));
            this.logger = serviceProvider.GetRequiredService<ILogger<ScopeMonitor>>()
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //this.logger?.MonitoringStartPending();
            if (this.started)
            {
                return Task.CompletedTask;
            }

            uint interval = 10 * 60; //seconds
            //using (var scope = this.ServiceProvider.CreateScope())
            //{
            //    var configurationManager =
            //        scope.ServiceProvider.GetRequiredService<IOptions<DocumentStoreConfiguration>>();

            //    // Минуты в секунды
            //    //interval = configurationManager.DocumentsMonitoringPeriod * 60;
            //    interval = this.configuration.DocumentsMonitoringPeriod * 60;
            //}

            if (interval == 0)
            {
                this.started = false;
                return Task.CompletedTask;
            }

            this.expirationMonitorTimer = new System.Timers.Timer
            {
                Interval = interval * 1000,
                AutoReset = false,
                Enabled = false
            };

            this.expirationMonitorTimer.Elapsed += async (sender, e) =>
            {
                try
                {
                    await this.ProcessAsync(cancellationToken);
                }
                finally
                {
                    ((System.Timers.Timer)sender).Enabled = true;
                }
            };

            this.expirationMonitorTimer.Start();

            this.started = true;

            //this.logger?.MonitoringStartComplited();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //this.logger?.MonitoringStopPending();
            if (!this.started)
            {
                return Task.CompletedTask;
            }

            this.started = false;
            this.expirationMonitorTimer.Stop();
            //this.logger?.MonitoringStopComplited();

            return Task.CompletedTask;
        }

        protected async Task ProcessAsync(CancellationToken stoppingToken)
        {
            try
            {
                var activeScopeId = this.dialogOrcestrator.GetScopeIds().Select(id => long.Parse(id));
                var activeUsers = await this.userStore.GetActiveUsersTelegramIdAsync(
                    activeScopeId.ToArray(),
                    (DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5)) // last 5 minute inactive
                        .ToUnixTimeSeconds());

                var scopeIdToDelete = activeScopeId.Where(s => !activeUsers.Contains(s)).ToArray();
                if (scopeIdToDelete.Length != 0)
                {
                    this.dialogOrcestrator.RemoveScopes(scopeIdToDelete);
                    this.logger.LogInformation($"Deleting inactive scopes for users [{string.Join(',', scopeIdToDelete)}]");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Error during scope cleanup: [{ex.ToString()}]");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.expirationMonitorTimer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ScopeMonitor()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}