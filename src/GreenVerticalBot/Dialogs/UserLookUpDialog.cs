using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    [AuthorizeRoles(UserRole.Admin)]
    internal class UserLookUpDialog : DialogBase
    {
        private IUserManager userManager;
        private ITaskManager taskManager;

        public UserLookUpDialog(
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            IUserManager userManager,
            ITaskManager taskManager,
            DialogContext data,
            ILogger<DialogBase> logger)
            : base(dialogOrcestrator, config, userManager, data, logger)
        {
            this.userManager = userManager
                ?? throw new ArgumentNullException(nameof(userManager));
            this.taskManager = taskManager
                ?? throw new ArgumentNullException(nameof(taskManager));
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }

        internal override async Task ProcessUpdateCoreAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var userId = update.Message?.Text;
            if (long.TryParse(userId, out var userIdLong))
            {
                var user = await this.userManager.GetUserByTelegramIdAsync(userIdLong);
                await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"{JsonConvert.SerializeObject(user, Formatting.Indented)}",
                            cancellationToken: cancellationToken);

                var tasks = await this.taskManager.GetTasksByLinkedObjectAsync(user.TelegramId.ToString());
                await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text: $"{JsonConvert.SerializeObject(tasks, Formatting.Indented)}",
                            cancellationToken: cancellationToken);
            }
        }
    }
}