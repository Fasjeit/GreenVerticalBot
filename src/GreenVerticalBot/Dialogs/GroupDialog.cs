using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Entities;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Transactions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    internal class GroupDialog : DialogBase
    {
        IUserManager userManager;
        ITaskManager taskManager;

        public GroupDialog(
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
            this.userManager = userManager
                ?? throw new ArgumentNullException(nameof(userManager));
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }

        internal override async Task ProcessUpdateCoreAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update!.Message!.Text!.StartsWith("/authenticate"))
            {
                var claim = new BotClaim(
                        type: ClaimTypes.Role,
                        value: UserRole.RegisteredUser.ToString(),
                        valueType: null,
                        issuer: "green_bot",
                        originalIssuer: null);

                var claims = new List<BotClaim>() { claim };


                // TransactionScopeAsyncFlowOption.Enabled - для async
                // ReadUncommitted - для избавления от дедлоков базы
                using (var transactionScope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted },
                    TransactionScopeAsyncFlowOption.Enabled))
                {
                    var user = this.Context.User;
                    if (user == null)
                    {
                        user = new()
                        {
                            TelegramId = this.Context.TelegramUserId,
                            Claims = claims,
                            Status = UserEntity.StatusFormats.Active
                        };
                        await this.userManager.AddUserAsync(user);
                    }
                    else
                    {
                        if (user.Claims.Any(c => c.Value == claim.Value))
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: user.TelegramId,
                                text: $"Права уже были получен!{Environment.NewLine}Используйте команда /user для просмотра профиля",
                                cancellationToken: cancellationToken);

                            return;
                        }
                        user = await this.userManager.GetUserByTelegramIdAsync(this.Context.TelegramUserId);
                        user.Claims.AddRange(claims);
                        await this.userManager.UpdateUserAsync(user);
                    }

                    // Создаём запись об успешной задаче выдаче утверждения
                    var task = new BotTask()
                    {
                        Status = StatusFormats.Approved,
                        LinkenObject = this.Context.TelegramUserId.ToString(),
                        Type = TaskType.RequestChatClaim,
                        Data = new TaskData() { Claims = claims },
                    };
                    await this.taskManager.AddTaskAsync(task);

                    transactionScope.Complete();

                    await botClient.SendTextMessageAsync(
                            chatId: user.TelegramId,
                            text: $"Регистрация пройдена!{Environment.NewLine}Используйте команда /user для просмотра профиля",
                            cancellationToken: cancellationToken);
                }
            }
        }
    }
}
