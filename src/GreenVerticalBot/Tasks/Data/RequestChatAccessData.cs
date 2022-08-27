using GreenVerticalBot.EntityFramework.Entities.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenVerticalBot.Tasks.Data
{
    internal class RequestChatAccessData : TaskData
    {
        public static TaskType Type = TaskType.RequestChatAccess;

        public string? ChatId
        {
            get
            {
                if (!TryGetValue(nameof(ChatId), out var chatId) ||
                    chatId == null)
                {
                    return null;
                }
                return (string)chatId;
            }
            set
            {
                this[nameof(ChatId)] = value;
            }
        }

        public string? InviteLink
        {
            get
            {
                if (!TryGetValue(nameof(this.InviteLink), out var InviteLink) ||
                    InviteLink == null)
                {
                    return null;
                }
                return (string)InviteLink;
            }
            set
            {
                this[nameof(InviteLink)] = value;
            }
        }
    }
}
