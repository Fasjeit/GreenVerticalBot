using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Extensions;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = GreenVerticalBot.Users.User;

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
                text: UserInfoDialog.FromatUserInfo(user, this.Context),
                cancellationToken: cancellationToken,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html );

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

        private static string FromatUserInfo(User user, DialogContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>Id:</b> {user.Id}");
            sb.AppendLine($"🆔 <b>TelegramId:</b> {user.TelegramId}");
            sb.AppendLine($"ℹ️ <b>Статус:</b> {user.Status}");
            sb.AppendLine($"🗂 <b>Права:</b>");
            foreach (var claim in context.Claims)
            {
                var claimValue = claim.Value;
                if (Enum.TryParse<UserRole>(claimValue, out var userRole))
                {
                    sb.AppendLine($"  *    {userRole.ToDescriptionString()}");
                }
            }

            sb.AppendLine($"🔑 <b>Приглашения:</b>");
            foreach (var invite in user.Data.Invites)
            {
                sb.AppendLine($"  *    <b>{invite.Despription}:</b> {invite.Value}");
            }
            return sb.ToString();
        }
    }
}