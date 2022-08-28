using GreenVerticalBot.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenVerticalBot.Tasks
{
    internal interface ITaskManager
    {
        /// <summary>
        /// Добавить новую задачу
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task AddTaskAsync(BotTask task);

        /// <summary>
        /// Получить информацию о задаче
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<BotTask?> GetTaskAsync(string taskId);

        /// <summary>
        /// Изменить данные задачи
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task UpdateTaskAsync(BotTask task);

        Task<BotTask[]> GetTasksByLinkedObjectAsync(string linkedObjectId);

        Task<BotTask[]> GetTasksToApproveByRequredClaimAsync(UserRole[] role);
    }
}
