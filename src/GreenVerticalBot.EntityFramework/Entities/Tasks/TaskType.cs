using System.ComponentModel;

namespace GreenVerticalBot.EntityFramework.Entities.Tasks
{
    public enum TaskType
    {
        [Description("")]
        NoType,
        [Description("Запрос потверждения роли")]
        RequestClaim,
        [Description("Запрос доступа к чату")]
        RequestChatAccess
    }
}
