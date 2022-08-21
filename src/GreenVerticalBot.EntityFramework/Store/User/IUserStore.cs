using GreenVerticalBot.EntityFramework.Entities;

namespace GreenVerticalBot.EntityFramework.Store.User
{
    public interface IUserStore
    {
        GreenVerticalBotContext Context { get; }

        /// <summary>
        /// Добавить запись с информацией о пользователе
        /// </summary>
        /// <param name="entity">
        /// Сущность для записи в БД
        /// </param>
        /// <returns></returns>
        Task AddUserAsync(UserEntity entity);

        /// <summary>
        /// Получить информацию о пользователе
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<UserEntity?> GetUserAsync(string userId);

        /// <summary>
        /// Получить информацию о пользователе
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<UserEntity?> GetUserByTelegramIdAsync(long telegramUserId);

        /// <summary>
        /// Изменить данные пользователя
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateUserAsync(UserEntity entity);
    }
}