using System;
using System.Diagnostics;

namespace StocksTicker.Loggers
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message, TraceEventType eventType)
        {
            Console.WriteLine("{0:G} - {1}: {2}", DateTime.Now, eventType, message);
        }
    }
}
