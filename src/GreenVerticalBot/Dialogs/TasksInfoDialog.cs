using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Extensions;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Tasks.Data;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    internal class TasksInfoDialog : DialogBase
    {
        ITaskManager taskManager;
        public TasksInfoDialog(
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            IUserManager userManager,
            ITaskManager taskManager,
            DialogContext context,
            ILogger<DialogBase> logger)
            : base(dialogOrcestrator, config, userManager, context, logger)
        {
            this.taskManager = taskManager
                ?? throw new ArgumentNullException(nameof(taskManager));
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }

        internal override async Task ProcessUpdateCoreAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var userId = this.Context.TelegramUserId.ToString();
            var userTasks = (await this.taskManager.GetTasksByLinkedObjectAsync(userId))
                .Where(t => t.CreationTime > DateTimeOffset.UtcNow - TimeSpan.FromDays(10));

            var sb = new StringBuilder();
            sb.AppendLine("<b>Запросы за последние 10 дней</b>");
            sb.AppendLine();

            sb.AppendLine("<b>Активные запросы:</b>");
            foreach (var task in userTasks.Where(t => t.Status == EntityFramework.Entities.Tasks.StatusFormats.Created))
            {
                sb.AppendLine(TasksInfoDialog.FormatTask(task));
            }
            sb.AppendLine();

            sb.AppendLine("<b>Подтверждённые запросы:</b>");
            foreach (var task in userTasks.Where(t => t.Status == EntityFramework.Entities.Tasks.StatusFormats.Approved))
            {
                sb.AppendLine(TasksInfoDialog.FormatTask(task));
            }
            sb.AppendLine();

            sb.AppendLine("<b>Отклонённые запросы:</b>");
            foreach (var task in userTasks.Where(t => t.Status == EntityFramework.Entities.Tasks.StatusFormats.Declined))
            {
                sb.AppendLine(TasksInfoDialog.FormatTask(task));
            }
            sb.AppendLine();

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: userId,
                text: sb.ToString(),
                cancellationToken: cancellationToken,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
        private static string FormatTask(BotTask task)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>❓ Запрос [{task.Id}]</b>");
            sb.AppendLine(task.Type.ToDescriptionString());
            sb.AppendLine(task.Status.ToDescriptionString());
            sb.AppendLine();

            if (task.Type == EntityFramework.Entities.Tasks.TaskType.RequestClaim)
            {
                sb.AppendLine($"Запрашиваемые права:");
                foreach (var claim in task.Data.ToRequestClaimTaskData().Claims)
                {
                    sb.AppendLine($"*    {Enum.Parse<UserRole>(claim.Value).ToDescriptionString()}");
                }
                var reason = task.Data.ToRequestClaimTaskData().Reason;
                sb.AppendLine($"Причина: {reason}");
            }
            return sb.ToString();
        }
    }
}
