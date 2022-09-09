using GreenVerticalBot.Authorization;
using GreenVerticalBot.EntityFramework.Entities.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GreenVerticalBot.Tasks.Data
{
    internal class RequestClaimTaskData : TaskData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public override TaskType Type => TaskType.RequestClaim;

        public List<BotClaim>? Claims { get; set; }

        public List<UserRole>? ShouldBeApprovedByAny { get; set; }

        public string? FileProofBase64 { get; set; }

        public string? FileProofName { get; set; }

        public string? Reason { get; set; }

        public string? UserDisplayName { get; set; }
    }
}
