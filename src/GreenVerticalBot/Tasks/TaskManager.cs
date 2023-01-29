using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.EntityFramework.Store.Tasks;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Tasks.Data;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System.Transactions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Update = Telegram.Bot.Types.Update;
using User = GreenVerticalBot.Users.User;

namespace GreenVerticalBot.Tasks
{
    internal class TaskManager : ITaskManager
    {
        private readonly ITaskStore taskStore;
        private readonly IUserManager userManager;
        private readonly ILogger<TaskManager> logger;

        public TaskManager(
            ITaskStore taskStore,
            IUserManager userManager,
            ILogger<TaskManager> logger)
        {
            this.taskStore = taskStore
                ?? throw new ArgumentNullException(nameof(taskStore));
            this.userManager = userManager
                ?? throw new ArgumentNullException(nameof(userManager));
            this.logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task AddTaskAsync(BotTask task)
        {
            await this.taskStore.AddTaskAsync(task.ToTaskEntity());
        }

        public async Task<BotTask?> GetTaskAsync(string taskId)
        {
            var taskEntity = await this.taskStore.GetTaskAsync(taskId);
            if (taskEntity == null)
            {
                return null;
            }
            return taskEntity.ToTask();
        }

        public async Task<BotTask[]> GetTasksByLinkedObjectAsync(string linkedObjectId)
        {
            var entities = await this.taskStore.GetTasksByLinkedObjectAsync(linkedObjectId);
            return entities.Select(e => e.ToTask()).ToArray();
        }

        public async Task<BotTask[]> GetTasksToApproveByRequredClaimAsync(UserRole[] roles)
        {
            var entities = await this.taskStore.GetToApproveTasks();
            var tasks = entities.Select(e => e.ToTask()).ToArray();
            return tasks.Where(
                t => t.Data.ToRequestClaimTaskData().ShouldBeApprovedByAny
                .Any(ur => roles.Contains(ur)))
                .ToArray();
        }

        public async Task<Invite> CreateApprovedInviteTask(
            ITelegramBotClient botClient,
            User user, 
            Update update, 
            ChatInfo chat)
        {
            // TransactionScopeAsyncFlowOption.Enabled - для async
            // ReadUncommitted - для избавления от дедлоков базы
            using (var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // Создаём задачу в бд
                var task = new BotTask()
                {
                    Status = TaskStatusFormats.Approved,
                    LinkedObject = user.TelegramId.ToString(),
                    Type = TaskType.RequestChatAccess,
                };

                var inviteLink = await botClient.CreateChatInviteLinkAsync(
                    new ChatId(
                        chat.ChatId),
                    name: StringFormatHelper.GetInviteString(
                        update.Message.From.Username,
                        user.TelegramId.ToString(),
                        chat.ChatId),
                    memberLimit: 1);

                // ставим линк и делаем запись в бд
                task.Data = new RequestChatAccessData() 
                { 
                    ChatId = chat.ChatId, 
                    InviteLink = inviteLink.InviteLink,
                    UserDisplayName = StringFormatHelper.GetUserDisplayName(update)
                };
                await this.AddTaskAsync(task);

                var userInvite = new Invite()
                {
                    Id = $"{inviteLink.Name}",
                    Despription = $"Приглашение в [{chat.FriendlyName}]",
                    Value = $"{inviteLink.InviteLink}"
                };

                user.Data.Invites.Add(userInvite);

                await this.userManager.UpdateUserAsync(user);

                this.logger.LogInformation($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : " +
                    $"new invite link generated [{inviteLink.Name} | value: {inviteLink.InviteLink}]");

                transactionScope.Complete();

                return userInvite;
            }
        }

        public async Task UpdateTaskAsync(BotTask task)
        {
            await this.taskStore.UpdateTaskAsync(task.ToTaskEntity());
        }
    }
}
