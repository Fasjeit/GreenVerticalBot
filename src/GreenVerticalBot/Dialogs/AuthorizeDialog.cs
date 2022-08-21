using GreenVerticalBot.Bot;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Entities;
using GreenVerticalBot.Helpers;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GreenVerticalBot.Dialogs
{
    internal class AuthorizeDialog : DialogBase
    {
        private AuthorizeDialogState state;

        private readonly IUserManager userManager;

        public AuthorizeDialog(
            DialogOrcestrator dialogOrcestrator, 
            AppConfig config, 
            IUserManager userManager,
            ILogger<AuthorizeDialog> logger) 
            : base(dialogOrcestrator, config, logger)
        {
            this.userManager = userManager;

            this.state = AuthorizeDialogState.Init;
        }

        public override async Task ProcessUpdateCore(
            ITelegramBotClient telegramBotClient,
            Update update,
            CancellationToken cancellationToken)
        {
            var user = await this.userManager.GetUserByTelegramIdAsync(update.Message.From.Id);
            if (user == null ||
                user.Status != UserEntity.StatusFormats.Active)
            {
                this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                        $": unauthorized");

                await telegramBotClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text:
                        $"Доступ запрещён.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);

                this.dialogOrcestrator.SwitchToDialog<WellcomeDialog>(
                    update.Message.From.Id.ToString(),
                    telegramBotClient,
                    update,
                    cancellationToken,
                    true);
                return;
            }
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
                                update.Message.From.Id.ToString(), 
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
                        var invites = user.Data.Invites;
                        invites.Add(
                            new Invite()
                            {
                                Id = $"{inviteLink.Name}",
                                Despription = "Приглашение в общий закрытый чат",
                                Value = $"{inviteLink.InviteLink}"
                            });
                        user.Data.Invites = invites;

                        await this.userManager.UpdateUserAsync(user);

                        this.logger.LogInformation($"user [{StringFormatHelper.GetUserIdForLogs(update)}] : " +
                            $"new invite link generated [{inviteLink.Name} | value: {inviteLink.InviteLink}]");

                        // Отправляем инвайт
                        Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                            chatId: update.Message.From.Id,
                            text: $"{inviteLink.InviteLink}",
                            cancellationToken: cancellationToken);

                        this.dialogOrcestrator.SwitchToDialog<WellcomeDialog>(
                            update.Message.From.Id.ToString(),
                            telegramBotClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                    else
                    {
                        this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                        $": unknown authorize target [{this.state}]");

                        await telegramBotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text:
                                $"Данная операция временно невозможна.",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                            cancellationToken: cancellationToken);
                        this.dialogOrcestrator.SwitchToDialog<WellcomeDialog>(
                            update.Message.From.Id.ToString(),
                            telegramBotClient,
                            update,
                            cancellationToken,
                            true);
                        return;
                    }
                }
                default:
                {
                    this.logger.LogError($"user [{StringFormatHelper.GetUserIdForLogs(update)}] " +
                        $": unknown authorize state [{this.state}]");

                    await telegramBotClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text:
                            $"Данная операция временно невозможна.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    this.dialogOrcestrator.SwitchToDialog<WellcomeDialog>(
                            update.Message.From.Id.ToString(),
                            telegramBotClient,
                            update,
                            cancellationToken,
                            true);
                    return;
                }
            }
        }

        public override Task ResetState()
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
