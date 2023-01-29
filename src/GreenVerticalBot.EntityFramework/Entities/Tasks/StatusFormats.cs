using System.ComponentModel;

namespace GreenVerticalBot.EntityFramework.Entities.Tasks
{
    public enum TaskStatusFormats
    {
        [Description("Создан, ожидает подтверждения")]
        Created,
        [Description("Подтвержден")]
        Approved,
        [Description("Отклонен")]
        Declined,
    }
}
