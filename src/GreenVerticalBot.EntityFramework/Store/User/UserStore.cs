using GreenVerticalBot.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenVerticalBot.EntityFramework.Store.User
{
    public class UserStore : IUserStore
    {
        public GreenVerticalBotContext Context { get; }

        private readonly EntityStore<UserEntity> userStore;

        public UserStore(GreenVerticalBotContext dbContext)
        {
            this.Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.userStore = new EntityStore<UserEntity>(dbContext);
        }

        public async Task AddUserAsync(UserEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.userStore.Create(entity);
            await this.Context.SaveChangesAsync();
        }

        public async Task<UserEntity?> GetUserAsync(string userId)
        {
            if (userId == null)
            {
                return null;
            }
            return await this.userStore.GetByIdAsync(userId);
        }

        public async Task<UserEntity?> GetUserByTelegramIdAsync(long telegramUserId)
        {
            return await this.userStore.EntitySetNoTracking.FirstOrDefaultAsync(
                u => u.TelegramId == telegramUserId);
        }

        public async Task UpdateUserAsync(UserEntity entity)
        {
            await this.userStore.UpdateAsync(entity);
        }

        public async Task<long[]> GetActiveUsersTelegramIdAsync(long[] telegramUserId, long thresholdTime)
        {
            return await this.userStore.EntitySetNoTracking.Where(
                u =>
                    telegramUserId.Contains(u.TelegramId) &&
                    u.LastAccessTime > thresholdTime)
                .Select(u => u.TelegramId).ToArrayAsync();
        }
    }
}