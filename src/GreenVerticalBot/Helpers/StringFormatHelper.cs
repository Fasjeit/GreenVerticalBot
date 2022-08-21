using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Helpers
{
    internal static class StringFormatHelper
    {
        public static string GetInviteString(
            string username,
            string userId,
            string targetChatId)
        {
            return $"{username}:aka:[{userId}]_to_[{targetChatId}]_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        }

        public static string GetUserIdForLogs(
            string username,
            string userId)
        {
            return $"{username}:aka:[{userId}]";
        }

        public static string GetUserIdForLogs(
            Update update)
        {
            if (update?.Message?.From == null)
            {
                throw new ArgumentException();
            }
            return $"{update.Message.From.Username}:aka:[{update.Message.From.Id}]";
        }
    }
}
