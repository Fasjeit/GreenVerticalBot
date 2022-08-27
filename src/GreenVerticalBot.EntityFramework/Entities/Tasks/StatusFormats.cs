using System.ComponentModel;

namespace GreenVerticalBot.EntityFramework.Entities.Tasks
{
    public enum StatusFormats
    {
        [Description("Создан, ожидает подтверждения")]
        Created,
        [Description("Подтверждена")]
        Approved,
        [Description("Отклонена")]
        Declined,
    }
}
