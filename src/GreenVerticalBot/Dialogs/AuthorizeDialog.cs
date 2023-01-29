using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Tasks;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

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
                    //var keyboard = new ReplyKeyboardMarkup(new KeyboardButton[][]
                    //        {
                    //    new[]
                    //    {
                    //        new KeyboardButton("/k9 Регистрация в закрытом чате 9-го корпуса (10 строительный)"),
                    //    },
                    //    new[]
                    //    {
                    //        new KeyboardButton("/general регистрация в общем закрытом чате жильцов"),
                    //    }
                    //        })
                    //{
                    //    OneTimeKeyboard = true,
                    //    ResizeKeyboard = true,
                    //};


                    var sb = new StringBuilder();
                    sb.AppendLine("Выберите чат для получения доступа:");
                    sb.AppendLine();

                    bool anyChats = false;
                    foreach (var chat in this.Config.ChatInfos)
                    {
                        if (this.Context.Claims.HasAllRolles(chat.Value.RequredClaims))
                        {
                            anyChats = true;
                            sb.AppendLine($"/{chat.Key} {chat.Value.FriendlyName}");
                        }
                    }

                    if (!anyChats)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: "Нет доступных чатов. Воспользуйтесь командой /authenticate для получения прав.",
                            cancellationToken: cancellationToken);
                        return;
                    }

                    this.Logger.LogInformation($"user [{StringFormatHelper.GetUserIdForLogs(update)}]: begin authorize");

                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: sb.ToString(),
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        //replyMarkup: keyboard,
                        cancellationToken: cancellationToken);

                    this.state = AuthorizeDialogState.SelectAuthorizationTarget;
                    return;
                }
                case AuthorizeDialogState.SelectAuthorizationTarget:
                {
                    var requestedChat = update?.Message?.Text;
                    if (requestedChat == null)
                    {
                        return;
                    }
                    if (!this.Config.ChatInfos.TryGetValue($"{requestedChat.Substring(1)}", out var chat))
                    {
                        return;
                    }

                    // проверяем права
                    if (!this.Context.Claims.HasAllRolles(chat.RequredClaims))
                    {
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                            $": unathorized access attempt to [{chat.ChatId}]");

                        await botClient.SendTextMessageAsync(
                            chatId: this.Context.ChatId,
                            text:
                                $"Попытка неавторизованного доступа. Недостаточно прав.{Environment.NewLine}" +
                                $"Требуются права:[{string.Join(',', chat.RequredClaims.Select(rc => rc.ToString()))}]",
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
                        // инвайта нет - создаём инвайт и таск
                        userInvite = await this.taskManager.CreateApprovedInviteTask(
                            botClient,
                            this.Context.User,
                            update,
                            chat);
                    }

                    // Отправляем инвайт
                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: this.Context.TelegramUserId,
                        text: $"<b>Ссылка для присоединения к чату:</b>{Environment.NewLine}" +
                        $"{userInvite.Value}",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
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