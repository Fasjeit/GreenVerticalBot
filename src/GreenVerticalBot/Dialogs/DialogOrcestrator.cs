using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Claims;
using Telegram.Bot;
using Update = Telegram.Bot.Types.Update;

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
        private ConcurrentDictionary<string, (IServiceScope dialogScope, DialogBase dialog)> dialogRecords;

        private bool disposedValue;

        /// <summary>
        /// Провайдер для dep inj
        /// </summary>
        private readonly IServiceProvider provider;

        private readonly ILogger<DialogOrcestrator> logger;

        private readonly BotConfiguration config;

        /// <summary>
        /// Создаёт оркестратор диалогов
        /// </summary>
        public DialogOrcestrator(
            IServiceProvider provider,
            BotConfiguration config,
            ILogger<DialogOrcestrator> logger)
        {
            this.provider = provider;
            this.logger = logger;
            this.config = config;
            this.dialogRecords = new();
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

            // Не обрабатываем сообщений, которые пришли ранее, чем минуту назад
            if (update?.Message?.Date < DateTime.UtcNow - TimeSpan.FromMinutes(1))
            {
                return;
            }

            if (update?.CallbackQuery?.Message?.Date < DateTime.UtcNow - TimeSpan.FromMinutes(1))
            {
                return;
            }

            var dialogId = DialogBase.GetDialogId(update).ToString();

            try
            {
                // Проверяем, есть ли активный диалог
                if (!this.dialogRecords.TryGetValue(dialogId, out var dialogRecord))
                {
                    // Диалога нет - создаём привественный диалог

                    // Создаём scope для диалога
                    var dialogScope = this.provider.CreateScope();

                    // Создаём провайдер в рамках scope
                    var serviceProvider = dialogScope.ServiceProvider;

                    var dialog = serviceProvider.GetRequiredService<WellcomeDialog>();

                    // Создаём запись о диалоге
                    dialogRecord = (dialogScope, dialog);

                    this.dialogRecords[dialogId] = dialogRecord;
                }

                {
                    // Выставляем конекст диалога
                    var dialog = dialogRecord.dialog;
                    await dialog.SetDialogContextAsync(botClient, update, cancellationToken);

                    // Проверка авторизации

                    bool isAuthorized = await this.AuthorizeAsync(dialog.GetType(), dialog.Context);
                    if (!isAuthorized)
                    {
                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                                $": unauthorized access to [{dialog.GetType().Name}]");

                        await botClient.SendTextMessageAsync(
                            chatId: dialog.Context.ChatId,
                            text:
                                $"Доступ запрещён.",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                            cancellationToken: cancellationToken);

                        await this.SwitchToDialogAsync<WellcomeDialog>(
                            dialog.Context.ChatId.ToString(),
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }

                    ///

                    // Если сообщение в групповом чате - обрабатываем отдельно в соотв. диалоге
                    if (DialogBase.IsGroupMessage(update))
                    {
                        // удаляем сообщение, т.к. оно в групповом чате
                        await dialog.Context.BotClient.DeleteMessageAsync(
                            dialog.Context.ChatId,
                            dialog.Context.Update.Message.MessageId,
                            cancellationToken);

                        await this.SwitchToDialogAsync<GroupDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }


                    ///

                    if (update?.Message?.Text == "/authenticate")
                    {
                        await this.SwitchToDialogAsync<AuthenticateDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else if (update?.Message?.Text == "/user")
                    {
                        await this.SwitchToDialogAsync<UserInfoDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else if (update?.Message?.Text == "/authorize")
                    {
                        await this.SwitchToDialogAsync<AuthorizeDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else if (update?.Message?.Text == "/help")
                    {
                        await this.SwitchToDialogAsync<WellcomeDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else if (update?.Message?.Text == "/tasks")
                    {
                        await this.SwitchToDialogAsync<TasksInfoDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else if (update?.Message?.Text == "/approve_task")
                    {
                        await this.SwitchToDialogAsync<ApproveDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else if (
                        update?.Message?.Text != null &&
                        update.Message.Text.StartsWith("/qr"))
                    {
                        await this.SwitchToDialogAsync<QrDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else if (update?.Message?.Text == "/a_userlookup")
                    {
                        await this.SwitchToDialogAsync<UserLookUpDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else if (update?.Message?.Text == "/a_status")
                    {
                        await this.SwitchToDialogAsync<ShowStatusDialog>
                            ($"{dialog.Context.ChatId}",
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
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
            if (!this.dialogRecords.ContainsKey(dialogId))
            {
                return;
                // TODO reset scope!!!!
            }
            var dialog = this.dialogRecords[dialogId].dialog;

            var isAuthorized = await this.AuthorizeAsync(typeof(T), dialog.Context);
            if (!isAuthorized)
            {
                this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                        $": unauthorized access to [{dialog.GetType().Name}]");

                await botClient.SendTextMessageAsync(
                    chatId: dialog.Context.ChatId,
                    text:
                        $"Доступ запрещён.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);

                await this.SwitchToDialogAsync<WellcomeDialog>(
                    dialog.Context.ChatId.ToString(),
                    botClient,
                    update,
                    cancellationToken,
                    true);
                return;
            }
            if (this.dialogRecords[dialogId].dialog.GetType() == typeof(T))
            {
                await this.dialogRecords[dialogId].dialog.ResetStateAsync();
            }

            // Создаём новый диалог
            var scope = this.dialogRecords[dialogId].dialogScope;
            var serviceProvider = scope.ServiceProvider;

            var newDialog = serviceProvider.GetRequiredService<T>();
            this.dialogRecords[dialogId] = (scope, newDialog);

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
            return this.dialogRecords.Keys;
        }

        public void RemoveScopes(long[] scopeIds)
        {
            foreach (var id in scopeIds)
            {
                this.dialogRecords.Remove(id.ToString(), out var value);
                value.dialogScope.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var dialog in this.dialogRecords.Values)
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

        private async Task<bool> AuthorizeAsync(
            Type dialogType,
            DialogContext context)
        {
            // Проверка авторизации
            bool isAuthorized = true;
            object[] attrs = dialogType.GetCustomAttributes(true);
            var rollesAttribute = attrs.FirstOrDefault(a => a is AuthorizeRolesAttribute);
            if (rollesAttribute != null)
            {
                if (context?.Claims == null)
                {
                    isAuthorized = false;
                }
                else
                {
                    isAuthorized = ((AuthorizeRolesAttribute)rollesAttribute).RoleArray
                        .Any(
                            r => context.Claims.Any(
                                dc => dc.Value == r.ToString()));
                }
            }
            return isAuthorized;
        }
    }
}