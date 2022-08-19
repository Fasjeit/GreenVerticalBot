using GreenVerticalBot.EntityFramework.Entities;

namespace GreenVerticalBot.EntityFramework.Store
{
    public interface ITasksStore
    {
        GreenVerticalBotContext Context { get; }

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