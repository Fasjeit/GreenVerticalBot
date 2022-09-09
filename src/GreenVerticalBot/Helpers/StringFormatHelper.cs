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
            return $"[{userId}]_to_[{targetChatId}]";
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

        public static string GetUserDisplayName(
            Update update)
        {
            if (update?.Message?.From == null)
            {
                throw new ArgumentException();
            }
            return $"@{update.Message.From.Username}:" +
                $"{update.Message.From.FirstName} {update.Message.From.LastName}" +
                $":id [{update.Message.From.Id}]";
        } 
    }
}