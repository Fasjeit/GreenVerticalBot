using GreenVerticalBot.Authorization;
using System.Security.Claims;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = GreenVerticalBot.Users.User;

namespace GreenVerticalBot.Dialogs
{
    internal class DialogData
    {
        public ITelegramBotClient BotClient { get; set; }
        public Update Update { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public long TelegramUserId { get; set; }
        public long ChatId => this.TelegramUserId;
        public User? User { get; set; }
        public List<BotClaim>? Claims { get; set;}
    }
}
