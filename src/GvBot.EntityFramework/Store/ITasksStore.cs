using GvBot.EntityFramework.Entities;

namespace GvBot.EntityFramework.Store
{
    public interface ITasksStore
    {
        GvBotContext Context { get; }

        /// <summary>
        /// Добавить запись с информацией о документе
        /// </summary>
        /// <param name="entity">
        /// Сущность для записи в БД
        /// </param>
        /// <returns></returns>
        Task AddTaskAsync(TaskEntity entity);
    }
}