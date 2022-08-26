using GreenVerticalBot.EntityFramework.Entities.Tasks;

namespace GreenVerticalBot.EntityFramework.Store.Tasks
{
    internal class StubTaskStore : ITaskStore
    {
        public GreenVerticalBotContext Context => throw new NotImplementedException();

        public Task AddTaskAsync(TaskEntity entity)
        {
            return Task.CompletedTask;
        }

        public Task<TaskEntity?> GetTaskAsync(string taskId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTaskAsync(TaskEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}