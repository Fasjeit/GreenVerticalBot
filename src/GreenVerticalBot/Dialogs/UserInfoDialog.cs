using GreenVerticalBot.Configuration;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    internal class UserInfoDialog : DialogBase
    {
        private IUserManager userManager;

        public UserInfoDialog(
            IUserManager userManager,
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            DialogContext data,
            ILogger<UserInfoDialog> logger)
            : base(dialogOrcestrator, config, userManager, data, logger)
        {
            this.userManager = userManager;
        }

        internal override async Task ProcessUpdateCoreAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var userId = DialogBase.GetUserId(update);

            var user = await this.userManager.GetUserByTelegramIdAsync(userId);
            if (user == null)
            {
                this.Logger.LogTrace($"user with telegramId [{update.Message.From.Id}] not found");
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $"Пользователь не найден!",
                    cancellationToken: cancellationToken);

                await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                    update.Message.From.Id.ToString(),
                    botClient,
                    update,
                    cancellationToken,
                    true);
                return;
            }
            this.Logger.LogTrace($"user with telegramId [{update.Message.From.Id}] found!");
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $"{JsonConvert.SerializeObject(user, Formatting.Indented)}",
                cancellationToken: cancellationToken);
            await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                update.Message.From.Id.ToString(),
                botClient,
                update,
                cancellationToken,
                true);
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }
    }
}