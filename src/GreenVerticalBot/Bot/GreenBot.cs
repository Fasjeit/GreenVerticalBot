using GreenVerticalBot.Configuration;
using GreenVerticalBot.Dialogs;
using GreenVerticalBot.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GreenVerticalBot.Bot
{
    internal class GreenBot
    {
        private AppConfig config;

        private DialogOrcestrator dialogOrcestrator;

        public GreenBot(AppConfig config, DialogOrcestrator dialogOrcestrator)
        {
            this.config = config;
            this.dialogOrcestrator = dialogOrcestrator;
        }

        public async Task MainRutine()
        {
            // Создаём клиент для общения с api телеграмма
            var botClient = new TelegramBotClient(this.config.BotToken);

            await botClient.SetMyCommandsAsync(new List<BotCommand>() { new BotCommand() { Command = @"/hello", Description = "some text" } });


            using var cts = new CancellationTokenSource();
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            // создаём обработчик сообщений бота
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                        pollingErrorHandler: HandlePollingErrorAsync,
                        receiverOptions: receiverOptions,
                        cancellationToken: cts.Token
                    );

            var me = await botClient.GetMeAsync();

            TimedConsoleLogger.WriteLine($"Start listening for @{me.Username}");

            // Отсанавливаем бота при нажатии enter в консоли
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                await this.dialogOrcestrator.ProcessToDialog(botClient, update, cancellationToken);
            }
            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                TimedConsoleLogger.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
        }
    }
}
