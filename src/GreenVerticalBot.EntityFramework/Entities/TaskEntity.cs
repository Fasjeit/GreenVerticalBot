using GreenVerticalBot.EntityFramework.Store;

namespace GreenVerticalBot.EntityFramework.Entities
{
    public class TaskEntity : IBaseEntity
    {
        public TaskEntity()
        {
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = TaskEntity.TaskTypes.RegisterUser;
        public long CreationTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public long UpdateTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Data { get; set; } = "{}";
        public string Status { get; set; } = TaskEntity.StatusFormats.Created;
        public string? LinkenObject { get; set; }

        public static class StatusFormats
        {
            public const string Created = "created";
            public const string Approved = "approved";
            public const string Declined = "declined";
        }

        public static class TaskTypes
        {
            public const string RegisterUser = "register_user";
        }
    }
}