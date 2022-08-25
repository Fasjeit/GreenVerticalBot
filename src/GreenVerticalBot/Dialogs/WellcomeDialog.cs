using GreenVerticalBot.Configuration;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    internal class WellcomeDialog : DialogBase
    {
        public WellcomeDialog(
            DialogOrcestrator dialogOrcestrator,
            AppConfig config,
            IUserManager userManager,
            DialogData data,
            ILogger<WellcomeDialog> logger)
            : base(dialogOrcestrator, config, userManager, data, logger)
        {
        }

        internal override async Task ProcessUpdateCoreAsync(
            ITelegramBotClient telegramBotClient, 
            Update update, 
            CancellationToken cancellationToken)
        {
            var userId = this.Data.TelegramUserId;

            // Выводим привественное сообщение    
            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: userId,
                text: $"Список команд:{Environment.NewLine}" +
                $"/register Регистрация жильца{Environment.NewLine}" +
                $"/user Просмотр профиля{Environment.NewLine}" +
                $"/authorize Получение доступа к чатам и ресурсам{Environment.NewLine}" +
                $"/help Вывод списка команд{Environment.NewLine}",
                cancellationToken: cancellationToken);
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }
    }
}
