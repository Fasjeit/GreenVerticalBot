using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    /// <summary>
    /// Класс управления диалогами
    /// </summary>
    internal class DialogOrcestrator
    {
        /// <summary>
        /// Список активных диалогов
        /// </summary>
        private ConcurrentDictionary<string, (IServiceScope dialogScope, DialogBase dialog)> dialogs;

        /// <summary>
        /// Провайдер для dep inj
        /// </summary>
        private readonly IServiceProvider provider;

        private readonly ILogger<DialogOrcestrator> logger;

        /// <summary>
        /// Создаёт оркестратор диалогов
        /// </summary>
        public DialogOrcestrator(
            IServiceProvider provider,
            ILogger<DialogOrcestrator> logger)
        {
            this.provider = provider;
            this.logger = logger;
            this.dialogs = new();
        }

        /// <summary>
        /// Передача сообщения в соотвествующий диалог для обраьотки
        /// </summary>
        /// <param name="update">Сообщение для обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public async Task ProcessToDialog(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) 
        {
            if (update.Message == null ||
                update.Message.From == null)
            {
                return;
            }

            // В качестве идентификатора используем идентификатор пользователя
            var dialogId = $"{update.Message.From.Id}";

            // Проверяем, есть ли активный диалог
            if (!this.dialogs.TryGetValue(dialogId, out var dialogRecord))
            {
                // Диалога нет - создаём привественный диалог

                // Создаём scope для диалога
                var dialogScope = this.provider.CreateScope();

                // Создаём провайдер в рамках scope
                var serviceProvider = dialogScope.ServiceProvider;

                // Создаём запись о диалоге
                dialogRecord = (dialogScope, serviceProvider.GetRequiredService<WellcomeDialog>());

                this.dialogs[dialogId] = dialogRecord;
            }

            // Обрабатываем сообщение в соответсвующем диалоге
            await dialogRecord.dialog.ProcessUpdate(botClient, update, cancellationToken);
        }

        public void SwitchToDialog<T>(
            string dialogId,
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken,
            bool proceed = false)
            where T : DialogBase
        {
            if (this.dialogs[dialogId].dialog.GetType() == typeof(T))
            {
                this.dialogs[dialogId].dialog.ResetState();
            }

            // Создаём новый диалог
            var scope = this.dialogs[dialogId].dialogScope;
            var serviceProvider = scope.ServiceProvider;

            var newDialog = serviceProvider.GetRequiredService<T>();
            this.dialogs[dialogId] = (scope, newDialog);

            // делаем ResetState на случай, если диалог уже был создан
            newDialog.ResetState();

            this.logger.LogTrace($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : goto {typeof(T).Name}");

            if (proceed)
            {
                newDialog.ProcessUpdateCore(botClient, update, cancellationToken);
            }
        }

        //private void SwitchUserDialog(string dialogId, DialogBase newDialog)
        //{
        //    if (this.dialogs.TryRemove(dialogId, out var oldDialog))
        //    {
        //        // dispose if needed
        //    }
        //    this.dialogs[dialogId] = newDialog;
        //}

        private void DialogCleanup()
        {
            // toDo clean old dialog states at timer time
            // using dialog.LastAccessTime
        }
    }
}
