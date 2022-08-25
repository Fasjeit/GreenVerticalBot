using GreenVerticalBot.Authorization;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;
using Telegram.Bot.Types.Enums;

namespace GreenVerticalBot.Dialogs
{
    /// <summary>
    /// Класс управления диалогами
    /// </summary>
    internal class DialogOrcestrator : IDisposable
    {
        /// <summary>
        /// Список активных диалогов
        /// </summary>
        private ConcurrentDictionary<string, (IServiceScope dialogScope, DialogBase dialog)> dialogs;
        private bool disposedValue;

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
        public async Task ProcessToDialog(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message &&
                update.CallbackQuery is not { } query)
            {
                return;
            }

            // В качестве идентификатора используем идентификатор пользователя
            var dialogId = DialogBase.GetUserId(update).ToString();

            try
            {
                // Проверяем, есть ли активный диалог
                if (!this.dialogs.TryGetValue(dialogId, out var dialogRecord))
                {
                    // Диалога нет - создаём привественный диалог

                    // Создаём scope для диалога
                    var dialogScope = this.provider.CreateScope();

                    // Создаём провайдер в рамках scope
                    var serviceProvider = dialogScope.ServiceProvider;

                    var dialog = serviceProvider.GetRequiredService<WellcomeDialog>();

                    // Создаём запись о диалоге
                    dialogRecord = (dialogScope, dialog);

                    this.dialogs[dialogId] = dialogRecord;
                }

                // Обрабатываем сообщение в соответсвующем диалоге
                await dialogRecord.dialog.ProcessUpdateAsync(botClient, update, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                           $": critial error - [{ex.ToString()}]");

                await botClient.SendTextMessageAsync(
                    chatId: dialogId,
                    text:
                        $"Произошла ошибка при работе бота. Попробуйте позднее.",
                             parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);

                // чистим скоуп, так как там уже не получится прогрузить бд
                this.RemoveScopes(new[] { long.Parse(dialogId) });
            }
        }

        public async Task SwitchToDialogAsync<T>(
            string dialogId,
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken,
            bool proceed = false)
            where T : DialogBase
        {
            if (!this.dialogs.ContainsKey(dialogId))
            {
                return;
                // TODO reset scope!!!!
            }
            if (this.dialogs[dialogId].dialog.GetType() == typeof(T))
            {
                await this.dialogs[dialogId].dialog.ResetStateAsync();
            }

            // Создаём новый диалог
            var scope = this.dialogs[dialogId].dialogScope;
            var serviceProvider = scope.ServiceProvider;

            var newDialog = serviceProvider.GetRequiredService<T>();
            this.dialogs[dialogId] = (scope, newDialog);

            // делаем ResetState на случай, если диалог уже был создан
            await newDialog.ResetStateAsync();

            this.logger.LogTrace($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : goto {typeof(T).Name}");

            if (proceed)
            {
                await newDialog.ProcessUpdateCoreAsync(botClient, update, cancellationToken);
            }
        }

        public ICollection<string> GetScopeIds()
        {
            return this.dialogs.Keys;
        }

        public void RemoveScopes(long[] scopeIds)
        {
            foreach (var id in scopeIds) 
            {
                this.dialogs.Remove(id.ToString(), out var value);
                value.dialogScope.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var dialog in this.dialogs.Values)
                    {
                        dialog.dialogScope.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DialogOrcestrator()
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
