using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Users;
using Newtonsoft.Json;

namespace GreenVerticalBot.Tasks
{
    internal class BotTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = TaskType.NoType;
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdateTime { get; set; } = DateTimeOffset.UtcNow;
        public TaskData Data { get; set; } = new TaskData();
        public string Status { get; set; } = StatusFormats.Created;
        public string? LinkedObject { get; set; }
    }

    internal static class TaskExtensions
    {
        public static TaskEntity ToTaskEntity(this BotTask task)
        {
            return new TaskEntity()
            {
                Id = task.Id,
                Type = task.Type,
                CreationTime = task.CreationTime.ToUnixTimeSeconds(),
                UpdateTime = task.UpdateTime.ToUnixTimeSeconds(),
                Data = JsonConvert.SerializeObject(task.Data),
                Status = task.Status,
                LinkedObject = task.LinkedObject,
            };
        }

        public static BotTask ToTask(this TaskEntity taskEntity)
        {
            return new BotTask()
            {
                Id = taskEntity.Id,
                Type = taskEntity.Type,
                CreationTime = DateTimeOffset.FromUnixTimeSeconds(taskEntity.CreationTime),
                UpdateTime = DateTimeOffset.FromUnixTimeSeconds(taskEntity.UpdateTime),
                Data = JsonConvert.DeserializeObject<TaskData>(taskEntity.Data),
                Status = taskEntity.Status,
                LinkedObject = taskEntity.LinkedObject,
            };
        }
    }
}
