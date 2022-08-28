using GreenVerticalBot.Authorization;
using GreenVerticalBot.Configuration;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
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
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("/authenticate Регистрация жильца");
            stringBuilder.AppendLine("/user Просмотр профиля");
            stringBuilder.AppendLine("/authorize Получение доступа к чатам и ресурсам:");
            stringBuilder.AppendLine("/tasks Вывод списка запросов");
            stringBuilder.AppendLine("/help Вывод списка команд");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Вспомагательные команды:");
            stringBuilder.AppendLine("/qr Генерация QR кода для строки");

            if (this.Context.Claims.HasRole(UserRole.Operator))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"Команды оператора:");
                stringBuilder.AppendLine("/approve_task Подтверждение заявок");
            }

            if (this.Context.Claims.HasRole(UserRole.Admin))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"Команды администратора:");
                stringBuilder.AppendLine("/a_userlookup Вывод имени пользователя по id telegram");
                stringBuilder.AppendLine("/a_status Вывод статуса бота");
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