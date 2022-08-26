using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GreenVerticalBot.Dialogs
{
    [AuthorizeRoles(UserRole.RegisteredUser)]
    internal class AuthorizeDialog : DialogBase
    {
        private AuthorizeDialogState state;

        private readonly IUserManager userManager;

        private readonly ITaskManager taskManager;

        public AuthorizeDialog(
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            IUserManager userManager,
            ITaskManager taskManager,
            DialogContext data,
            ILogger<AuthorizeDialog> logger)
            : base(dialogOrcestrator, config, userManager, data, logger)
        {
            this.userManager = userManager;
            this.taskManager = taskManager;

            this.state = AuthorizeDialogState.Init;
        }

        internal override async Task ProcessUpdateCoreAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            switch (this.state)
            {
                case AuthorizeDialogState.Init:
                {
                    var keyboard = new ReplyKeyboardMarkup(new KeyboardButton[][]
                            {
                        new[]
                        {
                            new KeyboardButton("/k9 Регистрация в закрытом чате 9-го корпуса (10 строительный)"),
                        },
                        new[]
                        {
                            new KeyboardButton("/general регистрация в общем закрытом чате жильцов"),
                        }
                            })
                    {
                        OneTimeKeyboard = true,
                        ResizeKeyboard = true,
                    };


                    var sb = new StringBuilder();
                    sb.AppendLine("Выберите Чат для регистрации:");
                    sb.AppendLine();
                    foreach (var chat in this.Config.ChatInfos)
                    {
                        sb.AppendLine($"/{chat.Key} {chat.Value.FriendlyName}");
                    }

                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: sb.ToString(),
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken);

                    this.state = AuthorizeDialogState.SelectAuthorizationTarget;
                    return;
                }
                case AuthorizeDialogState.SelectAuthorizationTarget:
                {
                    var requestedChat = update?.Message?.Text;
                    if (!this.Config.ChatInfos.TryGetValue($"{requestedChat.Substring(1)}", out var chat))
                    {
                        return;
                    }

                    // проверяем права
                    if (!this.Context.Claims.Any(c => c.Value == chat.RequredClaim))
                    {
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                            $": nathorized access to [{chat.ChatId}]");

                        await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text:
                                $"Попытка неавторизованного доступа. Недостаточно прав.",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                            cancellationToken: cancellationToken);
                        await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                            this.Context.TelegramUserId.ToString(),
                            botClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }

                    // проверяем, есть ли инвайт
                    var userInvite = this.Context.User.Data.Invites.FirstOrDefault(
                        i => 
                        i.Id == StringFormatHelper.GetInviteString(
                            update.Message.From.Username,
                            this.Context.TelegramUserId.ToString(),
                            chat.ChatId));

                    if (userInvite == null)
                    {
                        // Генерим инвайт линк на 1 использование
                        // NB. В идеале надо запоминать номер регистрации, id пользователя и инвайт.
                        // Если такой номер уже был - просто выдавать старый инвайт, а не генерить новый.

                        // Создаём задачу в бд
                        var task = new BotTask()
                        {
                            Status = StatusFormats.Approved,
                            LinkenObject = this.Context.TelegramUserId.ToString(),
                            Type = TaskType.RequestClaim,
                        };

                        var inviteLink = await botClient.CreateChatInviteLinkAsync(
                            new ChatId(
                                chat.ChatId),
                            name: StringFormatHelper.GetInviteString(
                                update.Message.From.Username,
                                this.Context.TelegramUserId.ToString(),
                                chat.ChatId),
                            memberLimit: 1);

                        // ставим линк и делаем запись в бд
                        task.Data = new TaskData() { ChatId = chat.ChatId, InviteLink = inviteLink.InviteLink };
                        await this.taskManager.AddTaskAsync(task);

                        // Перезаписываем, так как там оболочка вокруг словаря
                        var user = await this.userManager.GetUserByTelegramIdAsync(this.Context.TelegramUserId);

                        var invites = user.Data.Invites;
                        userInvite = new Invite()
                        {
                            Id = $"{inviteLink.Name}",
                            Despription = "Приглашение в общий закрытый чат",
                            Value = $"{inviteLink.InviteLink}"
                        };

                        invites.Add(userInvite);
                        user.Data.Invites = invites;

                        await this.userManager.UpdateUserAsync(user);

                        this.Logger.LogInformation($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : " +
                            $"new invite link generated [{inviteLink.Name} | value: {inviteLink.InviteLink}]");
                    }

                    // Отправляем инвайт
                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: this.Context.TelegramUserId,
                        text: $"{userInvite.Value}",
                        cancellationToken: cancellationToken);

                    await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                        this.Context.TelegramUserId.ToString(),
                        botClient,
                        update,
                        cancellationToken,
                        true);
                    return;
                }
                default:
                {
                    this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                        $": unknown authorize state [{this.state}]");

                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text:
                            $"Данная операция временно невозможна.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                            this.Context.TelegramUserId.ToString(),
                            botClient,
                            update,
                            cancellationToken,
                            true);
                    return;
                }
            }
        }

        public override Task ResetStateAsync()
        {
            this.state = AuthorizeDialogState.Init;
            return Task.CompletedTask;
        }

        public enum AuthorizeDialogState
        {
            Init = 0,
            SelectAuthorizationTarget,
        }
    }
}