﻿namespace GvBot
{
    internal static class TimedConsoleLogger
    {
        public static void WriteLine(string data)
        {
            Console.WriteLine($"{DateTime.UtcNow}: {data}");
        }
    }
}
