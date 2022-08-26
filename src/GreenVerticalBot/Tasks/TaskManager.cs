﻿using GreenVerticalBot.EntityFramework.Store.Tasks;
using Microsoft.Extensions.Logging;

namespace GreenVerticalBot.Tasks
{
    internal class TaskManager : ITaskManager
    {
        ITaskStore taskStore;
        ILogger<TaskManager> logger;

        public TaskManager(
            ITaskStore taskStore,
            ILogger<TaskManager> logger)
        {
            this.taskStore = taskStore
                ?? throw new ArgumentNullException(nameof(taskStore));
            this.logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task AddTaskAsync(BotTask task)
        {
            await this.taskStore.AddTaskAsync(task.ToTaskEntity());
        }

        public async Task<BotTask?> GetTaskAsync(string taskId)
        {
            var taskEntity = await this.taskStore.GetTaskAsync(taskId);
            if (taskEntity == null)
            {
                return null;
            }
            return taskEntity.ToTask();
        }

        public async Task<BotTask[]> GetTasksByLinkedObjectAsync(string linkedObjectId)
        {
            var entities = await this.taskStore.GetTasksByLinkedObjectAsync(linkedObjectId);
            return entities.Select(e => e.ToTask()).ToArray();
        }

        public async Task UpdateTaskAsync(BotTask task)
        {
            await this.taskStore.UpdateTaskAsync(task.ToTaskEntity());
        }
    }
}