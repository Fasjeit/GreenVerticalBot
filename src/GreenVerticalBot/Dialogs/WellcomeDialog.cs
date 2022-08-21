using GreenVerticalBot.Configuration;
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
            ILogger<WellcomeDialog> logger)
            : base(dialogOrcestrator, config, logger)
        {
        }

        public override async Task ProcessUpdateCore(
            ITelegramBotClient telegramBotClient, 
            Update update, 
            CancellationToken cancellationToken)
        {
            // Выводим привественное сообщение    
            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $"Список команд:{Environment.NewLine}" +
                $"/register Регистрация жильца{Environment.NewLine}" +
                $"/user Просмотр профиля{Environment.NewLine}" +
                $"/authorize Получение доступа к чатам и ресурсам{Environment.NewLine}",
                cancellationToken: cancellationToken);
        }

        public override Task ResetState()
        {
            return Task.CompletedTask;
        }
    }
}
