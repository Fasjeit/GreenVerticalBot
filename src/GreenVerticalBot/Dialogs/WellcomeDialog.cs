using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    internal class WellcomeDialog : DialogBase
    {
        public WellcomeDialog() 
            : base()
        {
        }

        public override async Task ProcessUpdate(ITelegramBotClient telegramBotClient, Update update, CancellationToken cnsToke)
        {
            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $"Wellcome!",
                    cancellationToken: cnsToke);
        }
    }
}
