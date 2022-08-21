using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Helpers
{
    internal static class MessageHelper
    {
        public static async Task<Message> SendSimpleMessage(
            TelegramBotClient telegramBotClient, 
            Update update, 
            string text, 
            CancellationToken cancellationToken)
        {
            return await telegramBotClient.SendTextMessageAsync(
                        chatId: update.Message!.Chat.Id,
                        text: text,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
        }
    }
}
