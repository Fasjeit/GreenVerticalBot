using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.EntityFramework.Store.Users;

namespace GreenVerticalBot.Users
{
    internal class UserManager : IUserManager
    {
        private readonly IUserStore userStore;
        private readonly BotConfiguration configuration;

        public UserManager(
            IUserStore userStore,
            BotConfiguration configuration)
        {
            this.userStore = userStore
                ?? throw new ArgumentNullException(nameof(userStore));
            this.configuration = configuration
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task AddUserAsync(User user)
        {
            await this.userStore.AddUserAsync(user.ToUserEntity());
        }

        public async Task<User?> GetUserAsync(string userId)
        {
            var userEntity = await this.userStore.GetUserAsync(userId);
            return userEntity?.ToUser();
        }

        public async Task<User?> GetUserByTelegramIdAsync(long telegramUserId)
        {
            var userEntity = await this.userStore.GetUserByTelegramIdAsync(telegramUserId);
            return userEntity?.ToUser();
        }

        public Task<List<string>> GetUsersIdWithRoleFromExtraClaimsAsync(UserRole role)
        {
            var userIds = this.configuration.ExtraClaims
                .Where(
                    ec => ec.Value.Any(
                        r => r == role.ToString()))
                .Select(ec => ec.Key).ToList();
            return Task.FromResult(userIds.ToList());
        }

        public async Task UpdateUserAsync(User user)
        {
            await this.userStore.UpdateUserAsync(user.ToUserEntity());
        }
    }
}