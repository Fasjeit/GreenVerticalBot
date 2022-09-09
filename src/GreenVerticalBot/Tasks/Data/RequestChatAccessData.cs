using GreenVerticalBot.EntityFramework.Entities.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenVerticalBot.Tasks.Data
{
    internal class RequestChatAccessData : TaskData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public override TaskType Type => TaskType.RequestChatAccess;

        public string? ChatId { get; set; }

        public string? InviteLink { get; set; }

        public string UserDisplayName { get; set; }
    }
}
