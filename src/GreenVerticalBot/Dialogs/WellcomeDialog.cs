using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    internal class WellcomeDialog : DialogBase
    {
        public WellcomeDialog(
            DialogOrcestrator dialogOrcestrator,
            BotConfiguration config,
            IUserManager userManager,
            DialogContext data,
            ILogger<WellcomeDialog> logger)
            : base(dialogOrcestrator, config, userManager, data, logger)
        {
        }

        internal override async Task ProcessUpdateCoreAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            var userId = this.Context.TelegramUserId;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Список команд:");
            stringBuilder.AppendLine("/register Регистрация жильца");
            stringBuilder.AppendLine("/user Просмотр профиля");
            stringBuilder.AppendLine("/authorize Получение доступа к чатам и ресурсам:");
            stringBuilder.AppendLine("/help Вывод списка команд");

            if (this.Context.Claims.HasRole(UserRole.Admin))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"Команды администратора:");
                stringBuilder.AppendLine("/a_userlookup Вывод имени пользователя по id telegram");
            }

            // Выводим привественное сообщение
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: userId,
                text: stringBuilder.ToString(),
                cancellationToken: cancellationToken);
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }
    }
}