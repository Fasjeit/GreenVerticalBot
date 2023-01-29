using GreenVerticalBot.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace GreenVerticalBot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await GreenVerticalBotHost.RunHost();
            return;
        }
    }
}