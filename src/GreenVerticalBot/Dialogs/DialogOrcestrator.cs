using Microsoft.Extensions.DependencyInjection;
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
        private ConcurrentDictionary<string, DialogBase> dialogs;

        /// <summary>
        /// Провайдер для dep inj
        /// </summary>
        private readonly IServiceProvider provider;

        /// <summary>
        /// Создаёт оркестратор диалогов
        /// </summary>
        public DialogOrcestrator(IServiceProvider provider)
        {
            this.provider = provider;
            this.dialogs = new();
        }

        /// <summary>
        /// Передача сообщения в соотвествующий диалог для обраьотки
        /// </summary>
        /// <param name="update">Сообщение для обработки</param>
        /// <param name="cnsToke">Токен отмены</param>
        public async Task ProcessToDialog(ITelegramBotClient botClient, Update update, CancellationToken cnsToke) 
        {
            if (update.Message == null ||
                update.Message.From == null)
            {
                return;
            }

            // В качестве идентификатора используем идентификатор чата и пользователя
            var dialogId = $"{update.Message.Chat.Id}_{update.Message.From.Id}";

            // Проверяем, есть ли активный диалог
            if (!this.dialogs.TryGetValue(dialogId, out var dialog))
            {
                // Диалога нет - создаём привественный диалог
                var dialogScope = new DialogScope(this.provider);
                var serviceProvider = dialogScope.ServiceScope.ServiceProvider;
                dialog = serviceProvider.GetRequiredService<WellcomeDialog>();

                this.dialogs[dialogId] = dialog;
            }

            // Обрабатываем сообщение в соответсвующем диалоге
            await dialog.ProcessUpdate(botClient, update, cnsToke);
        }

        //public void SwitchToDialog<T>(string dialogId, Dictionary<string, object> dialogData) 
        //    where T : DialogBase
        //{
        //    var newDialog = (T)(Activator.CreateInstance(typeof(T), dialogData) ?? throw new ArgumentException(nameof(T)));
        //    this.SwitchUserDialog(dialogId, newDialog);
        //}

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
