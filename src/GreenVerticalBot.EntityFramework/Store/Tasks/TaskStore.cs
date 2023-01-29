using GreenVerticalBot.EntityFramework.Entities.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace GreenVerticalBot.EntityFramework.Store.Tasks
{
    public class TaskStore : ITaskStore
    {
        public GreenVerticalBotContext Context { get; }

        private readonly EntityStore<TaskEntity> tasksStore;

        public TaskStore(GreenVerticalBotContext dbContext)
        {
            this.Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.tasksStore = new EntityStore<TaskEntity>(dbContext);
        }

        public async Task AddTaskAsync(TaskEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.tasksStore.Create(entity);
            await this.Context.SaveChangesAsync();
        }

        public async Task<TaskEntity?> GetTaskAsync(string taskId)
        {
            if (taskId == null)
            {
                return null;
            }
            return await this.tasksStore.GetByIdAsync(taskId);
        }

        public Task UpdateTaskAsync(TaskEntity entity)
        {
            entity.UpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return this.tasksStore.UpdateAsync(entity);
        }

        public Task<TaskEntity[]> GetTasksByLinkedObjectAsync(string linkedObjectId)
        {
            return this.tasksStore.EntitySetNoTracking.Where(t => t.LinkedObject == linkedObjectId).ToArrayAsync();
        }

        public Task<TaskEntity[]> GetToApproveTasks()
        {
            return this.tasksStore.EntitySetNoTracking.Where(t => t.Status == TaskStatusFormats.Created.ToString()).ToArrayAsync();
        }
    }
}