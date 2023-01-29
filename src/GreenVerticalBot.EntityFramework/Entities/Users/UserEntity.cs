using GreenVerticalBot.EntityFramework.Store;

namespace GreenVerticalBot.EntityFramework.Entities.Users
{
    public class UserEntity : IBaseEntity
    {
        public UserEntity()
        {
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long TelegramId { get; set; } = 0;
        public long CreationTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public long LastAccessTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Claims { get; set; } = string.Empty;
        public string Data { get; set; } = "{}";
        public string Status { get; set; } = UserStatusFormats.Active;        
    }
}