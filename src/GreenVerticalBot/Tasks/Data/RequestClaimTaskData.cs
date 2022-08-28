using GreenVerticalBot.Authorization;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Users;
using Newtonsoft.Json.Linq;

namespace GreenVerticalBot.Tasks.Data
{
    internal class RequestClaimTaskData : TaskData
    {
        public static TaskType Type = TaskType.RequestClaim;

        public RequestClaimTaskData()
        {
        }

        public RequestClaimTaskData(Dictionary<string, object> dictionary)
            : base(dictionary)
        {
        }

        public List<BotClaim>? Claims
        {
            get
            {
                if (!TryGetValue(nameof(Claims), out var claims) ||
                    claims == null)
                {
                    return new List<BotClaim>();
                }
                if (claims is JArray invitesArray)
                {
                    claims = invitesArray.ToObject<List<BotClaim>>();
                }
                return (List<BotClaim>)claims;
            }
            set
            {
                this[nameof(Claims)] = value;
            }
        }

        public List<UserRole>? ShouldBeApprovedByAny
        {
            get
            {
                if (!TryGetValue(nameof(ShouldBeApprovedByAny), out var shouldApprovedBy) ||
                    shouldApprovedBy == null)
                {
                    return new List<UserRole>();
                }
                if (shouldApprovedBy is JArray shouldApprovedByArray)
                {
                    shouldApprovedBy = shouldApprovedByArray.ToObject<List<UserRole>>();
                }
                return (List<UserRole>)shouldApprovedBy;
            }
            set
            {
                this[nameof(ShouldBeApprovedByAny)] = value;
            }
        }

        public string? FileProofBase64
        {
            get
            {
                if (!TryGetValue(nameof(this.FileProofBase64), out var proof) ||
                    proof == null)
                {
                    return null;
                }
                return (string)proof;
            }
            set
            {
                this[nameof(FileProofBase64)] = value;
            }
        }

        public string? FileProofName
        {
            get
            {
                if (!TryGetValue(nameof(this.FileProofName), out var name) ||
                    name == null)
                {
                    return null;
                }
                return (string)name;
            }
            set
            {
                this[nameof(FileProofName)] = value;
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
