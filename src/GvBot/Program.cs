﻿using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using GvBot.RestModels;
using GvBot.Logs;
using GvBot.Configuration;

namespace GvBot
{
    class Program
    {
        /// <summary>
        /// Клиент для общения с сервисом проверки подписи
        /// </summary>
        private static readonly HttpClient SvsCLient = new HttpClient();

        static async Task Main(string[] args)
        {
            await GvBotHost.RunHost();
            return;            
        }
    }
}