using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Helpers
{
    internal static class MessageHelper
    {
        public static async Task<Message> SendSimpleMessage(
            TelegramBotClient botClient,
            Update update,
            string text,
            CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                        chatId: update.Message!.Chat.Id,
                        text: text,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
        }
    }
}