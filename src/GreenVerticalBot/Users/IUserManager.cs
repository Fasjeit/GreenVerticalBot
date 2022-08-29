namespace GreenVerticalBot.Users
{
    internal interface IUserManager
    {
        /// <summary>
        /// Добавить нового пользователя
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task AddUserAsync(User user);

        /// <summary>
        /// Получить информацию о пользователе
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        //Task<User?> GetUserAsync(string telegramUserId);

        /// <summary>
        /// Получить информацию о пользователе по Id Telegram
        /// </summary>
        /// <param name="telegramUserId"></param>
        /// <returns></returns>
        Task<User?> GetUserByTelegramIdAsync(long telegramUserId);

        /// <summary>
        /// Изменить данные пользователя
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task UpdateUserAsync(User user);
    }
}