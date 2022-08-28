using GreenVerticalBot.Authorization;
using GreenVerticalBot.Users;
using Newtonsoft.Json.Linq;

namespace GreenVerticalBot.Tasks.Data
{
    internal class TaskData : Dictionary<string, object>
    {
        public TaskData()
        {
        }

        public TaskData(Dictionary<string, object> dictionary)
            : base(dictionary)
        {
        }

        public RequestChatAccessData ToRequestChatAccessData()
        {
            return new RequestChatAccessData(this);
        }

        public RequestClaimTaskData ToRequestClaimTaskData()
        {
            return new RequestClaimTaskData(this);
        }
    }
}