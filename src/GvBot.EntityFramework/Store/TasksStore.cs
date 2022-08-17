namespace GvBot.EntityFramework.Store
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using GvBot.EntityFramework.Entities;
    using Microsoft.EntityFrameworkCore;

    public class TasksStore : ITasksStore
    {
        protected const int LastAccessTimeDeltaSeconds = 60 * 60 * 24;

        public GvBotContext Context { get; }

        private readonly EntityStore<TaskEntity> tasksStore;

        public TasksStore(GvBotContext dbContext)
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