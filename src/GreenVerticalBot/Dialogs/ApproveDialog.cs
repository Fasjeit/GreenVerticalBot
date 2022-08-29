using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Entities;
using GreenVerticalBot.Extensions;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Tasks.Data;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    [AuthorizeRoles(UserRole.Operator)]
    internal class ApproveDialog : DialogBase
    {
        ApproveDialogState state;
        ITaskManager taskManager;
        IUserManager userManager;

        public ApproveDialog(
            DialogOrcestrator dialogOrcestrator, 
            BotConfiguration config, 
            IUserManager userManager, 
            ITaskManager taskManager,
            DialogContext context, 
            ILogger<DialogBase> logger) 
            : base(dialogOrcestrator, config, userManager, context, logger)
        {
            this.userManager = userManager
                ?? throw new ArgumentNullException(nameof(userManager));
            this.taskManager = taskManager
                ?? throw new ArgumentNullException(nameof(taskManager));
        }

        public override Task ResetStateAsync()
        {
            this.state = ApproveDialogState.Iitial;
            return Task.CompletedTask;
        }

        internal override async Task ProcessUpdateCoreAsync(
            ITelegramBotClient botClient, 
            Update update, 
            CancellationToken cancellationToken)
        {
            switch (this.state)
            {
                case ApproveDialogState.Iitial:
                {
                    var tasksToApprove = await this.taskManager.GetTasksToApproveByRequredClaimAsync(
                        this.Context.Claims.Select(c => Enum.Parse<UserRole>(c.Value)).ToArray());
                    

                    // #Q_ tmp filter
                    tasksToApprove = tasksToApprove
                        .Where(
                            tta => tta.Type == EntityFramework.Entities.Tasks.TaskType.RequestClaim)
                        .ToArray();

                    if (tasksToApprove.Length == 0)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: this.Context.TelegramUserId,
                            text: "Нет запросов на подтверждение :)",
                            cancellationToken: cancellationToken);
                        return;
                    }

                    await botClient.SendTextMessageAsync(
                        chatId: this.Context.TelegramUserId,
                        text:
                            $"Ожидающих запросов - [{tasksToApprove.Length}]",
                        cancellationToken: cancellationToken);


                    var taskToApprove = tasksToApprove[0];

                    var data = taskToApprove.Data.ToRequestClaimTaskData();

                    var sb = new StringBuilder();
                    sb.AppendLine($"<b> ❓ Запрос [{taskToApprove.Id}]</b>");
                    sb.AppendLine($"<b>Идентификатор объекта</b> [{taskToApprove.LinkedObject}]");
                    sb.AppendLine($"<b>Тип запроса</b> [{taskToApprove.Type.ToDescriptionString()}]");
                    sb.AppendLine($"<b>Запрашиваемые права:</b>");
                    foreach (var claim in data.Claims)
                    {
                        sb.AppendLine($"*    {Enum.Parse<UserRole>(claim.Value).ToDescriptionString()}");
                    }

                    this.Context.ContextDataObject["requested_claims"] = data.Claims;
                    this.Context.ContextDataString["task_id"] = taskToApprove.Id;

                    await botClient.SendTextMessageAsync(
                        chatId: this.Context.TelegramUserId,
                        text: sb.ToString(),
                        cancellationToken: cancellationToken,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                    var fileBasr64 = data.FileProofBase64;
                    var fileBytes = Convert.FromBase64String(fileBasr64);

                    using (var stream = new MemoryStream(fileBytes))
                    {
                        await botClient.SendDocumentAsync(
                            chatId: this.Context.TelegramUserId,
                            document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, data.FileProofName),
                            cancellationToken: cancellationToken);
                    }

                    await botClient.SendTextMessageAsync(
                        chatId: this.Context.TelegramUserId,
                        text: 
                            $"/approve - подтверждение запроса{Environment.NewLine}" +
                            $"/reject - отклонение запроса",
                        cancellationToken: cancellationToken);

                    this.state = ApproveDialogState.ReadResult;
                    return;
                }
                case ApproveDialogState.ReadResult:
                {
                    var taskId = this.Context.ContextDataString["task_id"];

                    if (update?.Message?.Text == "/approve")
                    {
                        // TransactionScopeAsyncFlowOption.Enabled - для async
                        // ReadUncommitted - для избавления от дедлоков базы
                        using (var transactionScope = new TransactionScope(
                            TransactionScopeOption.Required,
                            new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted },
                            TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var requestedClaims = (List<BotClaim>)this.Context.ContextDataObject["requested_claims"];

                            var newUserClaims = new List<BotClaim>();
                            // пересоздаём утрверждения с новым источником
                            foreach (var claim in requestedClaims)
                            {
                                newUserClaims.Add(new BotClaim(
                                    claim.Type,
                                    claim.Value,
                                    claim.ValueType,
                                    claim.Issuer,
                                    this.Context.TelegramUserId.ToString()));
                            }
                            
                            var task = await this.taskManager.GetTaskAsync(taskId);
                            if (task.Status != EntityFramework.Entities.Tasks.StatusFormats.Created)
                            {
                                this.Logger.LogInformation(
                                    $"operator [{StringFormatHelper.GetUserIdForLogs(update)}]: " +
                                    $"task [{taskId}] already processed");

                                // write already approved or rejected
                                await botClient.SendTextMessageAsync(
                                    chatId: this.Context.TelegramUserId,
                                    text: $"Запрос уже обработан! ",
                                    cancellationToken: cancellationToken);

                                this.state = ApproveDialogState.Iitial;
                                await this.ProcessUpdateCoreAsync(botClient, update, cancellationToken);
                                return;
                            }
                            task.Status = EntityFramework.Entities.Tasks.StatusFormats.Approved;
                            task.Data = new RequestClaimTaskData()
                            {
                                Claims = newUserClaims,
                            };

                            await this.taskManager.UpdateTaskAsync(task);

                            var user = await this.userManager.GetUserByTelegramIdAsync(long.Parse(task.LinkedObject));
                            if (user == null)
                            {
                                this.Logger.LogInformation(
                                    $"operator [{StringFormatHelper.GetUserIdForLogs(update)}]:" +
                                    $"user [{task.LinkedObject}] does not exist, creating....");

                                user = new()
                                {
                                    TelegramId = long.Parse(task.LinkedObject),
                                    Status = UserEntity.StatusFormats.Active
                                };
                                await this.userManager.AddUserAsync(user);

                                //// write user does not exists
                                //await botClient.SendTextMessageAsync(
                                //    chatId: this.Context.TelegramUserId,
                                //    text: $"Пользователь не существует! ",
                                //    cancellationToken: cancellationToken);

                                //this.state = ApproveDialogState.Iitial;
                                //await this.ProcessUpdateCoreAsync(botClient, update, cancellationToken);
                                //return;
                            }

                            // добавляем только новые
                            // Claims реализует свой компаратор по значению. Должен работать
                            newUserClaims
                                .Where(uc => !user.Claims.Contains(uc))
                                .ToList()
                                .ForEach(uc => user.Claims.Add(uc));

                            await this.userManager.UpdateUserAsync(user);

                            this.Logger.LogInformation(
                                    $"operator [{StringFormatHelper.GetUserIdForLogs(update)}]:" +
                                    $"task [{taskId}] for user [{task.LinkedObject}] approved");

                            await botClient.SendTextMessageAsync(
                                chatId: this.Context.TelegramUserId,
                                text: $"Запрос подтверждён! ",
                                cancellationToken: cancellationToken);

                            // notify user
                            await botClient.SendTextMessageAsync(
                                chatId: task.LinkedObject,
                                text:
                                    $"Ваш запрос [{task.Id}] подтверждён!{Environment.NewLine}" +
                                    $"Воспользуйтесь комнадой /authenticate для получения доступа",
                                cancellationToken: cancellationToken);

                            this.state = ApproveDialogState.Iitial;
                            await this.ProcessUpdateCoreAsync(botClient, update, cancellationToken);

                            transactionScope.Complete();
                            return;
                        }
                    }
                    else if (update?.Message?.Text == "/reject")
                    {
                        await botClient.SendTextMessageAsync(
                                chatId: this.Context.TelegramUserId,
                                text: $"Опишите причину отклонения, её увидит пользователь",
                                cancellationToken: cancellationToken);
                        this.state = ApproveDialogState.ReadReason;
                        return;
                    }
                    else 
                    {
                        // write helper text
                        return;
                    }
                    return;
                }
                case ApproveDialogState.ReadReason:
                {
                    if (string.IsNullOrEmpty(update?.Message?.Text))
                    {
                        await botClient.SendTextMessageAsync(
                                chatId: this.Context.TelegramUserId,
                                text: $"Опишите причину отклонения, её увидит пользователь",
                                cancellationToken: cancellationToken);
                        return;
                    }
                    var reason = update.Message.Text;
                    var taskId = this.Context.ContextDataString["task_id"];
                    var task = await this.taskManager.GetTaskAsync(taskId);
                    if (task.Status != EntityFramework.Entities.Tasks.StatusFormats.Created)
                    {
                        // write already approved or rejected
                        this.state = ApproveDialogState.Iitial;
                        return;
                    }
                    task.Status = EntityFramework.Entities.Tasks.StatusFormats.Declined;
                    task.Data = new RequestClaimTaskData()
                    {
                        Claims = task.Data.ToRequestClaimTaskData().Claims,
                        Reason = reason, 
                    };

                    await this.taskManager.UpdateTaskAsync(task);

                    this.Logger.LogInformation(
                                   $"operator [{StringFormatHelper.GetUserIdForLogs(update)}]:" +
                                   $"task [{taskId}] for user [{task.LinkedObject}] declined for a reason [{reason}]");

                    await botClient.SendTextMessageAsync(
                            chatId: this.Context.TelegramUserId,
                            text: $"Запрос отклонён! ",
                            cancellationToken: cancellationToken);

                    // notify user
                    await botClient.SendTextMessageAsync(
                        chatId: task.LinkedObject,
                        text:
                            $"Ваш запрос [{task.Id}] отклонён!{Environment.NewLine}" +
                            $"Причина: [{reason}]",
                        cancellationToken: cancellationToken);

                    this.state = ApproveDialogState.Iitial;
                    await this.ProcessUpdateCoreAsync(botClient, update, cancellationToken);
                    return;
                }
            }
        }

        private enum ApproveDialogState
        {
            Iitial,
            ReadResult,
            ReadReason
        }
    }
}
