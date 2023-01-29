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

            if (update?.Message?.Text == "/start")
            {
                var introString = 
                    $"1. Используйте команду `/authenticate` в личном чате с ботом для выбора корпуса и загрузки файла подтверждения{Environment.NewLine}{Environment.NewLine}" +
                    $"2. После создания запроса на регистрацию ожидайте проверки оператором. Бот уведомит вас о результатах проверки.{Environment.NewLine}" +
                    $"Статус запроса можно проверить через команду `/tasks`{Environment.NewLine}{Environment.NewLine}" +
                    $"3. После подтверждения запроса оператором воспользуйтесь командой `/authorize` для получения доступа к закрытому чату.{Environment.NewLine}" +
                    $"Список приглашений в закрытые чаты можно просмотреть через команду `/user`{Environment.NewLine}";

                await botClient.SendTextMessageAsync(
                    chatId: userId,
                    text: introString,
                    cancellationToken: cancellationToken);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"🆔 Ваш идентификатор: [{userId}]");
            stringBuilder.AppendLine("Список команд:");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("/authenticate Регистрация жильца");
            stringBuilder.AppendLine("/user Просмотр профиля");
            stringBuilder.AppendLine("/authorize Получение доступа к чатам и ресурсам");
            stringBuilder.AppendLine("/tasks Вывод списка запросов");
            stringBuilder.AppendLine("/help Вывод списка команд");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Вспомагательные команды:");
            stringBuilder.AppendLine("/qr Генерация QR кода для строки");

            if (this.Context.Claims.HasRole(UserRole.Operator))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"Команды оператора:");
                stringBuilder.AppendLine("/o_approve Подтверждение заявок");
            }

            if (this.Context.Claims.HasRole(UserRole.Admin))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"Команды администратора:");
                stringBuilder.AppendLine("/a_userlookup Вывод имени пользователя по id telegram");
                stringBuilder.AppendLine("/a_status Вывод статуса бота");
                stringBuilder.AppendLine("/a_logs Последние записи журнала бота");
            }

            // Выводим привественное сообщение
            await botClient.SendTextMessageAsync(
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