using GreenVerticalBot.EntityFramework.Entities.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GreenVerticalBot.Tasks.Data
{
    internal class TaskData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual TaskType Type { get; protected set; } = TaskType.NoType;

        public static Type GetDataType(TaskType taskType)
        {
            return taskType switch
            {
                TaskType.RequestChatAccess => typeof(RequestChatAccessData),
                TaskType.RequestClaim => typeof(RequestClaimTaskData),
                _ => typeof(TaskData),
            };
        }

        internal RequestClaimTaskData ToRequestClaimTaskData()
        {
            return (RequestClaimTaskData)this;
        }

        internal RequestChatAccessData ToRequestChatAccessData()
        {
            return (RequestChatAccessData)this;
        }
    }
}