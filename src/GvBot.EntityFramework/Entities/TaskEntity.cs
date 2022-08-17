using GvBot.EntityFramework.Store;

namespace GvBot.EntityFramework.Entities
{
    public class TaskEntity : IBaseEntity
    {
        public TaskEntity()
        {
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long CreationTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Data { get; set; } = "{}";
        public string Status { get; set; } = TaskEntity.StatusFormats.Created;

        public static class StatusFormats
        {
            public const string Created = "created";
            public const string Approved = "approved";
            public const string Declined = "declined";
        }
    }
}