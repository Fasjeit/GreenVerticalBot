using GreenVerticalBot.Authorization;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Users;
using Newtonsoft.Json.Linq;

namespace GreenVerticalBot.Tasks.Data
{
    internal class RequestClaimTaskData : TaskData
    {
        public static TaskType Type = TaskType.RequestClaim;

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

        public string? FileProof
        {
            get
            {
                if (!TryGetValue(nameof(this.FileProof), out var proof) ||
                    proof == null)
                {
                    return null;
                }
                return (string)proof;
            }
            set
            {
                this[nameof(FileProof)] = value;
            }
        }

        public string? Reason
        {
            get
            {
                if (!TryGetValue(nameof(this.Reason), out var reason) ||
                    reason == null)
                {
                    return null;
                }
                return (string)reason;
            }
            set
            {
                this[nameof(Reason)] = value;
            }
        }
    }
}
