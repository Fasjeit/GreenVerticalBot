using GreenVerticalBot.EntityFramework.Entities;
using GreenVerticalBot.EntityFramework.Store.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenVerticalBot.Users
{
    internal class UserManager : IUserManager
    {
        private readonly IUserStore userStore;

        public UserManager(IUserStore userStore)
        {
            this.userStore = userStore;
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

        public async Task UpdateUserAsync(User user)
        {
            await this.userStore.UpdateUserAsync(user.ToUserEntity());
        }
    }
}
