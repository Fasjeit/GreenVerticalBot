using GreenVerticalBot.Authorization;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = GreenVerticalBot.Users.User;

namespace GreenVerticalBot.Dialogs
{
    internal class DialogContext
    {

        public DialogContext()
        {
        }

        public ITelegramBotClient BotClient { get; set; }
        public Update Update { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public long TelegramUserId { get; set; }
        public long ChatId { get; set; }
        public User? User { get; set; }
        public List<BotClaim> Claims { get; set; } = new List<BotClaim>();
        public bool IsGroupMessage => this.ChatId == this.TelegramUserId;

        public Dictionary<string, string> ContextDataString = new Dictionary<string, string>();

        public Dictionary<string, object> ContextDataObject = new Dictionary<string, object>();
    }
}