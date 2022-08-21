using GreenVerticalBot.EntityFramework.Entities;

namespace GreenVerticalBot.EntityFramework.Store.Tasks
{
    public interface ITaskStore
    {
        GreenVerticalBotContext Context { get; }

        /// <summary>
        /// Добавить запись с информацией о задаче
        /// </summary>
        /// <param name="entity">
        /// Сущность для записи в БД
        /// </param>
        /// <returns></returns>
        Task AddTaskAsync(TaskEntity entity);
    }
}