﻿using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Entities;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Tasks.Data;
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
            if (update?.Message?.Text is string { } text && text.StartsWith("/chatId"))
            {
                if (this.Context.Claims.HasRole(UserRole.Admin))
                {
                    await botClient.SendTextMessageAsync(
                            chatId: this.Context.TelegramUserId,
                            text: $"ChatId is [{this.Context.ChatId}]",
                            cancellationToken: cancellationToken);
                }
                return;
            }
            if (update?.Message?.Text is string { } textS && textS.StartsWith("/authenticate"))
            {

                var chatId = this.Context.ChatId;

                // Получаем список ролей, которые надо выдать
                var chatInfo = this.Config.ChatInfos.FirstOrDefault(ci => ci.Value.ChatId == chatId.ToString());
                var requiredRoles = chatInfo.Value.RequredClaims;

                var claims = new List<BotClaim>();
                foreach (var requiredRole in requiredRoles)
                {
                    var claim = new BotClaim(
                        type: ClaimTypes.Role,
                        value: requiredRole.ToString(),
                        valueType: null,
                        issuer: "green_bot",
                        originalIssuer: null);
                    claims.Add(claim);
                }

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
                        bool newClaims = false;
                        user = await this.userManager.GetUserByTelegramIdAsync(this.Context.TelegramUserId);
                        foreach (var claim in claims)
                        {
                            // добавляем только новые утрверждения
                            if (!user.Claims.Any(c => c.Value == claim.Value))
                            {
                                user.Claims.Add(claim);
                                newClaims = true;
                            }
                        }
                        if (newClaims)
                        {
                            await this.userManager.UpdateUserAsync(user);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: user.TelegramId,
                                text: $"Права уже были получен!{Environment.NewLine}Используйте команда /user для просмотра профиля",
                                cancellationToken: cancellationToken);
                            return;
                        }
                    }

                    // Создаём запись об успешной задаче выдаче утверждения
                    var task = new BotTask()
                    {
                        Status = StatusFormats.Approved,
                        LinkedObject = this.Context.TelegramUserId.ToString(),
                        Type = TaskType.RequestClaim,
                        Data = new RequestClaimTaskData() { Claims = claims },
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
