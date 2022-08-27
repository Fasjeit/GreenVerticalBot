using GreenVerticalBot.EntityFramework.Entities.Tasks;
using GreenVerticalBot.Tasks.Data;
using Newtonsoft.Json;

namespace GreenVerticalBot.Tasks
{
    internal class BotTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public TaskType Type { get; set; } = TaskType.NoType;
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdateTime { get; set; } = DateTimeOffset.UtcNow;
        public TaskData Data { get; set; } = new TaskData();
        public StatusFormats Status { get; set; } = StatusFormats.Created;
        public string? LinkedObject { get; set; }
    }

    internal static class TaskExtensions
    {
        public static TaskEntity ToTaskEntity(this BotTask task)
        {
            return new TaskEntity()
            {
                Id = task.Id,
                Type = task.Type.ToString(),
                CreationTime = task.CreationTime.ToUnixTimeSeconds(),
                UpdateTime = task.UpdateTime.ToUnixTimeSeconds(),
                Data = JsonConvert.SerializeObject(task.Data),
                Status = task.Status.ToString(),
                LinkedObject = task.LinkedObject,
            };
        }

        public static BotTask ToTask(this TaskEntity taskEntity)
        {
            return new BotTask()
            {
                Id = taskEntity.Id,
                Type = Enum.Parse<TaskType>(taskEntity.Type),
                CreationTime = DateTimeOffset.FromUnixTimeSeconds(taskEntity.CreationTime),
                UpdateTime = DateTimeOffset.FromUnixTimeSeconds(taskEntity.UpdateTime),
                Data = JsonConvert.DeserializeObject<TaskData>(taskEntity.Data),
                Status = Enum.Parse<StatusFormats>(taskEntity.Status),
                LinkedObject = taskEntity.LinkedObject,
            };
        }
    }
}
