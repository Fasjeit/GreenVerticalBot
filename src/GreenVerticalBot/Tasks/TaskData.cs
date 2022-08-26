using GreenVerticalBot.Authorization;
using Newtonsoft.Json.Linq;

namespace GreenVerticalBot.Users
{
    internal class TaskData : Dictionary<string, object>
    {
        public List<BotClaim>? Claims
        {
            get
            {
                if (!this.TryGetValue(nameof(this.Claims), out var invites) ||
                    invites == null)
                {
                    return new List<BotClaim>();
                }
                if (invites is JArray invitesArray)
                {
                    invites = invitesArray.ToObject<List<Invite>>();
                }
                return (List<BotClaim>)invites;
            }
            set
            {
                this[nameof(this.Claims)] = value;
            }
        }

        public string? ChatId
        {
            get
            {
                if (!this.TryGetValue(nameof(this.ChatId), out var chatId) ||
                    chatId == null)
                {
                    return null;
                }
                return (string)chatId;
            }
            set
            {
                this[nameof(this.ChatId)] = value;
            }
        }

        public string? InviteLink
        {
            get
            {
                if (!this.TryGetValue(nameof(this.InviteLink), out var InviteLink) ||
                    InviteLink == null)
                {
                    return null;
                }
                return (string)InviteLink;
            }
            set
            {
                this[nameof(this.InviteLink)] = value;
            }
        }
    }
}