using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
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

        public AuthorizeDialog(
            DialogOrcestrator dialogOrcestrator,
            AppConfig config,
            IUserManager userManager,
            DialogData data,
            ILogger<AuthorizeDialog> logger)
            : base(dialogOrcestrator, config, userManager, data, logger)
        {
            this.userManager = userManager;

            this.state = AuthorizeDialogState.Init;
        }

        internal override async Task ProcessUpdateCoreAsync(
            ITelegramBotClient telegramBotClient,
            Update update,
            CancellationToken cancellationToken)
        {
            var userId = DialogBase.GetUserId(update);

            //var user = await this.userManager.GetUserByTelegramIdAsync(userId);
            //if (user == null ||
            //    user.Status != UserEntity.StatusFormats.Active &&
            //    this.Data.ClaimsPrincipal.IsInRole("RegisteredUser"))
            //{
            //    this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
            //            $": unauthorized");

            //    await telegramBotClient.SendTextMessageAsync(
            //        chatId: update.Message.Chat.Id,
            //        text:
            //            $"Доступ запрещён.",
            //        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            //        cancellationToken: cancellationToken);

            //    await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
            //        userId.ToString(),
            //        telegramBotClient,
            //        update,
            //        cancellationToken,
            //        true);
            //    return;
            //}
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
                        OneTimeKeyboard = true
                    };

                    await telegramBotClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text:
                            $"Выберите Чат для регистрации:{Environment.NewLine}{Environment.NewLine}" +
                            $" /k9 Регистрация в закрытом чате 9-го корпуса (10 строительный){Environment.NewLine}{Environment.NewLine}" +
                            $" /general регистрация в общем закрытом чате жильцов",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken);

                    this.state = AuthorizeDialogState.SelectAuthorizationTarget;
                    return;
                }
                case AuthorizeDialogState.SelectAuthorizationTarget:
                {
                    if (update.Message.Text.StartsWith("/general"))
                    {
                        // Генерим инвайт линк на 1 использование
                        // NB. В идеале надо запоминать номер регистрации, id пользователя и инвайт.
                        // Если такой номер уже был - просто выдавать старый инвайт, а не генерить новый.

                        var inviteLink = await telegramBotClient.CreateChatInviteLinkAsync(
                            new ChatId(
                                this.Config.PrivateChatId),
                            name: StringFormatHelper.GetInviteString(
                                update.Message.From.Username,
                                userId.ToString(),
                                this.Config.PrivateChatId.ToString()),
                            memberLimit: 1);

                        //// Делаем запись в бд
                        //await this.taskStore.AddTaskAsync(
                        //    new TaskEntity()
                        //    {
                        //        Status = TaskEntity.StatusFormats.Approved,
                        //        Data = JsonConvert.SerializeObject(data)
                        //    });

                        // Перезаписываем, так как там оболочка вокруг словаря
                        var invites = this.Data!.User!.Data.Invites;
                        invites.Add(
                            new Invite()
                            {
                                Id = $"{inviteLink.Name}",
                                Despription = "Приглашение в общий закрытый чат",
                                Value = $"{inviteLink.InviteLink}"
                            });
                        this.Data.User.Data.Invites = invites;

                        await this.userManager.UpdateUserAsync(this.Data.User);

                        this.Logger.LogInformation($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : " +
                            $"new invite link generated [{inviteLink.Name} | value: {inviteLink.InviteLink}]");

                        // Отправляем инвайт
                        Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                            chatId: userId,
                            text: $"{inviteLink.InviteLink}",
                            cancellationToken: cancellationToken);

                        await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                            userId.ToString(),
                            telegramBotClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else
                    {
                        this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                        $": unknown authorize target [{this.state}]");

                        await telegramBotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text:
                                $"Данная операция временно невозможна.",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                            cancellationToken: cancellationToken);
                        await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                            userId.ToString(),
                            telegramBotClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                }
                default:
                {
                    this.Logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                        $": unknown authorize state [{this.state}]");

                    await telegramBotClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text:
                            $"Данная операция временно невозможна.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    await this.dialogOrcestrator.SwitchToDialogAsync<WellcomeDialog>(
                            userId.ToString(),
                            telegramBotClient,
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