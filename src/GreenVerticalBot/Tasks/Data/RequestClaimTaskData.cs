using GreenVerticalBot.Authorization;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Users;
using Newtonsoft.Json.Linq;

namespace GreenVerticalBot.Tasks.Data
{
    internal class RequestClaimTaskData : TaskData
    {
        public static string Type = TaskType.RequestClaim;

        public List<BotClaim>? Claims
        {
            get
            {
                if (!TryGetValue(nameof(Claims), out var invites) ||
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
                this[nameof(Claims)] = value;
            }
        }
    }
}
