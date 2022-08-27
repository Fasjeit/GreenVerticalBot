using GreenVerticalBot.EntityFramework.Store;

namespace GreenVerticalBot.EntityFramework.Entities.Tasks
{
    public class TaskEntity : IBaseEntity
    {
        public TaskEntity()
        {
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = TaskType.NoType;
        public long CreationTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public long UpdateTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Data { get; set; } = "{}";
        public string Status { get; set; } = StatusFormats.Created;
        public string? LinkedObject { get; set; }
    }
}