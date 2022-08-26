using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    [AuthorizeRoles(UserRole.Admin)]
    internal class ShowStatusDialog : DialogBase
    {
        private IUserManager userManager;

        public ShowStatusDialog(
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            IUserManager userManager,
            DialogContext data,
            ILogger<DialogBase> logger)
            : base(dialogOrcestrator, config, userManager, data, logger)
        {
            this.userManager = userManager
                ?? throw new ArgumentNullException(nameof(userManager));
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }

        internal override async Task ProcessUpdateCoreAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var scopes = this.dialogOrcestrator.GetScopeIds();
            await botClient.SendTextMessageAsync(
                        chatId: this.Context.ChatId,
                        text: $"{JsonConvert.SerializeObject(scopes, Formatting.Indented)}",
                        cancellationToken: cancellationToken);
            await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(this.Context.ChatId.ToString(), botClient, update, cancellationToken);
        }
    }
}