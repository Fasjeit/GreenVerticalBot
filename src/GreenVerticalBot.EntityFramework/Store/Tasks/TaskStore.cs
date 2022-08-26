using GreenVerticalBot.EntityFramework.Entities.Tasks;

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
            return await this.tasksStore.GetByIdAsync(taskId);
        }

        public async Task UpdateTaskAsync(TaskEntity entity)
        {
            await this.tasksStore.UpdateAsync(entity);
        }
    }
}