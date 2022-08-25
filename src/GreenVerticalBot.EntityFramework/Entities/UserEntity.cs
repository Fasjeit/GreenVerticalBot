using GreenVerticalBot.EntityFramework.Store;

namespace GreenVerticalBot.EntityFramework.Entities
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
        public string Building { get; set; } = UserEntity.BuildingId.None;
        public string Data { get; set; } = "{}";
        public string Status { get; set; } = UserEntity.StatusFormats.Active;

        public static class StatusFormats
        {
            public const string New = "new";
            public const string Active = "active";
            public const string Blocked = "blocked";
        }

        public static class ClaimString
        {
            public const string Owner = "owner";
            public const string Admin = "admin";
            public const string Operator = "operator";
            public const string User = "user";
            public const string UnauthorizedUser = "unauthorized_user";
        }

        public static class BuildingId
        {
            public const string None = "none";

            /// <summary>
            /// 10 строительный, 9 по адресу
            /// </summary>
            public const string B10_9 = "b10_9";

        }
    }
}
