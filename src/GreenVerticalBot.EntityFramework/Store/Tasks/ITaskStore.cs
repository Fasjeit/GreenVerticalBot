using GreenVerticalBot.EntityFramework.Entities.Tasks;

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

        /// <summary>
        /// Получить информацию о задаче
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<TaskEntity?> GetTaskAsync(string taskId);

        Task<TaskEntity[]> GetTasksByLinkedObjectAsync(string linkedObjectId);

        /// <summary>
        /// Изменить данные пользователя
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateTaskAsync(TaskEntity entity);
    }
}