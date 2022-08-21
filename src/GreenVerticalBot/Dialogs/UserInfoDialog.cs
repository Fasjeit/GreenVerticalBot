using GreenVerticalBot.Configuration;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    internal class UserInfoDialog : DialogBase
    {
        private IUserManager userManager;

        public UserInfoDialog(
            IUserManager userManager,
            DialogOrcestrator dialogOrcestrator, 
            AppConfig config,
            ILogger<UserInfoDialog> logger) 
            : base(dialogOrcestrator, config, logger)
        {
            this.userManager = userManager;
        }

        public override async Task ProcessUpdateCore(ITelegramBotClient telegramBotClient, Update update, CancellationToken cancellationToken)
        {
            var user = await this.userManager.GetUserByTelegramIdAsync(update.Message.From.Id);
            if (user == null)
            {
                this.logger.LogTrace($"user with telegramId [{update.Message.From.Id}] not found");
                await telegramBotClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $"Пользователь не найден!",
                    cancellationToken: cancellationToken);
                return;
            }

            this.logger.LogTrace($"user with telegramId [{update.Message.From.Id}] found!");
            await telegramBotClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $"{JsonConvert.SerializeObject(user, Formatting.Indented)}",
                cancellationToken: cancellationToken);
            this.dialogOrcestrator.SwitchToDialog<WellcomeDialog>(
                update.Message.From.Id.ToString(), 
                telegramBotClient, 
                update, 
                cancellationToken,
                true);
        }

        public override Task ResetState()
        {
            return Task.CompletedTask;
        }
    }
}
