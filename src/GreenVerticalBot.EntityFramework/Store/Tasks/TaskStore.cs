using GreenVerticalBot.EntityFramework.Entities;

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
    }
}