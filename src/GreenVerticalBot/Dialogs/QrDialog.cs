﻿using GreenVerticalBot.Configuration;
using GreenVerticalBot.Users;
using Microsoft.Extensions.Logging;
using Net.Codecrete.QrCodeGenerator;
using Svg;
using System.Collections;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GreenVerticalBot.Dialogs
{
    internal class QrDialog : DialogBase
    {
        public QrDialog(DialogOrcestrator dialogOrcestrator, BotConfiguration config, IUserManager userManager, DialogContext context, ILogger<DialogBase> logger) : base(dialogOrcestrator, config, userManager, context, logger)
        {
        }

        public override Task ResetStateAsync()
        {
            return Task.CompletedTask;
        }

        internal override async Task ProcessUpdateCoreAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var data = update.Message.Text;
            data = data.Replace(@"/qr", string.Empty);
            if (data.StartsWith(" "))
            {
                data = data.Replace(@" ", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                await botClient.SendTextMessageAsync(
                    chatId: this.Context.TelegramUserId,
                    text: "Использование: /qr текст_для_содержимого",
                    cancellationToken: cancellationToken);
                return;
            }

            var qr = QrCode.EncodeText(data, QrCode.Ecc.Medium);
            string svg = qr.ToSvgString(1);
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(svg)))
            {
                var svgDocument = SvgDocument.Open<SvgDocument>(stream: stream);
                using (var bitmap = svgDocument.Draw(256, 256))
                {
                    using (var bmpStream = new MemoryStream())
                    {
                        bitmap.Save(bmpStream, System.Drawing.Imaging.ImageFormat.Png);
                        bmpStream.Position = 0;

                        await botClient.SendPhotoAsync(
                        chatId: this.Context.TelegramUserId,
                        photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(bmpStream),
                        cancellationToken: cancellationToken);
                    }
                }
            }
        }
    }
}