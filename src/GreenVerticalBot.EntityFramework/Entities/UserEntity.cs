using GreenVerticalBot.EntityFramework.Store;

namespace GreenVerticalBot.EntityFramework.Entities
{
    public class UserEntity : IBaseEntity
    {
        public UserEntity()
        {
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long CreationTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Role { get; set; } = UserEntity.UserRols.UnauthorizedUser;
        public string Building { get; set; } = UserEntity.BuildingId.None;
        public string Data { get; set; } = "{}";
        public string Status { get; set; } = UserEntity.StatusFormats.Active;

        public static class StatusFormats
        {
            public const string Active = "active";
            public const string Blocked = "blocked";
        }

        public static class UserRols
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
            public const string B10_9 = "b10_9";

        }
    }
}
