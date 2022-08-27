using GreenVerticalBot.Authorization;
using GreenVerticalBot.Users;
using Newtonsoft.Json.Linq;

namespace GreenVerticalBot.Tasks.Data
{
    internal class TaskData : Dictionary<string, object>
    {
        //public List<BotClaim>? Claims
        //{
        //    get
        //    {
        //        if (!TryGetValue(nameof(Claims), out var invites) ||
        //            invites == null)
        //        {
        //            return new List<BotClaim>();
        //        }
        //        if (invites is JArray invitesArray)
        //        {
        //            invites = invitesArray.ToObject<List<Invite>>();
        //        }
        //        return (List<BotClaim>)invites;
        //    }
        //    set
        //    {
        //        this[nameof(Claims)] = value;
        //    }
        //}

        //public string? ChatId
        //{
        //    get
        //    {
        //        if (!TryGetValue(nameof(ChatId), out var chatId) ||
        //            chatId == null)
        //        {
        //            return null;
        //        }
        //        return (string)chatId;
        //    }
        //    set
        //    {
        //        this[nameof(ChatId)] = value;
        //    }
        //}

        //public string? InviteLink
        //{
        //    get
        //    {
        //        if (!TryGetValue(nameof(this.InviteLink), out var InviteLink) ||
        //            InviteLink == null)
        //        {
        //            return null;
        //        }
        //        return (string)InviteLink;
        //    }
        //    set
        //    {
        //        this[nameof(InviteLink)] = value;
        //    }
        //}
    }
}