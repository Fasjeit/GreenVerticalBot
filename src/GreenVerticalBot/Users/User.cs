using GreenVerticalBot.EntityFramework.Entities;
using Newtonsoft.Json;
using System.Text.Json;

namespace GreenVerticalBot.Users
{
    internal class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long TelegramId { get; set; } = 0;
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastAccessTime { get; set; } = DateTimeOffset.UtcNow;
        public string Role { get; set; } = UserEntity.UserRols.UnauthorizedUser;
        public string Building { get; set; } = UserEntity.BuildingId.None;
        public UserData Data { get; set; } = new ();
        public string Status { get; set; } = UserEntity.StatusFormats.Active;
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
                Role = user.Role,
                Building = user.Building,
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
                Role = userEntity.Role,
                Building = userEntity.Building,
                Data = JsonConvert.DeserializeObject<UserData>(userEntity.Data),
                Status = userEntity.Status,
            };
        }
    }
}
