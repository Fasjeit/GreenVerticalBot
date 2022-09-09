using GreenVerticalBot.Configuration;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Logging;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace GreenVerticalBot.Dialogs
{
    internal class LogsDialog : DialogBase
    {
        public LogsDialog(
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            IUserManager userManager,
            DialogContext context,
            ILogger<DialogBase> logger)
            : base(dialogOrcestrator, config, userManager, context, logger)
        {
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }

        internal override async Task ProcessUpdateCoreAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            this.Logger.LogInformation($"admin [{StringFormatHelper.GetUserIdForLogs(update)}]: request logs");
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var evt in InMemorySink.Events.ToList())
                    {
                        await writer.WriteAsync(evt);
                    }
                    if (stream.Position == 0)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: this.Context.TelegramUserId,
                            text: "Нет логов :(",
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        stream.Position = 0;
                        await botClient.SendDocumentAsync(
                                    chatId: this.Context.TelegramUserId,
                                    document: new InputOnlineFile(stream, $"logs_{DateTimeOffset.Now}.txt"),
                                    cancellationToken: cancellationToken);
                    }
                }
            }
            await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                this.Context.ChatId.ToString(),
                botClient,
                update,
                cancellationToken);
        }
    }
}
