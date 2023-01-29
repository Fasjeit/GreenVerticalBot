using GreenVerticalBot.Authorization;
using GreenVerticalBot.EntityFramework.Entities.Users;
using GreenVerticalBot.Users.Data;
using Newtonsoft.Json;

namespace GreenVerticalBot.Users
{
    internal class User
    {
        /// <summary>
        /// Идентификатор пользователя в бд
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Идентификатор пользоватьеля в телеграме
        /// </summary>
        public long TelegramId { get; set; } = 0;

        /// <summary>
        /// Дата создания пользователя
        /// </summary>
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Дата последнего обращения ползователя с точностью до минуты
        /// </summary>
        public DateTimeOffset LastAccessTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Утверждения пользователя
        /// </summary>
        public List<BotClaim> Claims { get; set; } = new List<BotClaim>();

        /// <summary>
        /// Прочие данные пользователя
        /// </summary>
        public UserData Data { get; set; } = new();

        /// <summary>
        /// Статус пользователя
        /// </summary>
        public string Status { get; set; } = UserStatusFormats.Active;
    }

    internal static class UserExtensions
    {
        public static UserEntity ToUserEntity(this User user)
        {
            return new UserEntity()
            {
                Id = user.Id,
                TelegramId = user.TelegramId,
                CreationTime = user.CreationTime.ToUnixTimeSeconds(),
                LastAccessTime = user.LastAccessTime.ToUnixTimeSeconds(),
                Claims = JsonConvert.SerializeObject(user.Claims),
                Data = JsonConvert.SerializeObject(user.Data),
                Status = user.Status,
            };
        }

        public static User ToUser(this UserEntity userEntity)
        {
            return new User()
            {
                Id = userEntity.Id,
                TelegramId = userEntity.TelegramId,
                CreationTime = DateTimeOffset.FromUnixTimeSeconds(userEntity.CreationTime),
                LastAccessTime = DateTimeOffset.FromUnixTimeSeconds(userEntity.LastAccessTime),
                Claims = JsonConvert.DeserializeObject<List<BotClaim>>(userEntity.Claims),
                Data = JsonConvert.DeserializeObject<UserData>(userEntity.Data),
                Status = userEntity.Status,
            };
        }
    }
}