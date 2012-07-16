using System.Diagnostics;

namespace StocksTicker.Loggers
{
    public class NullLogger : ILogger
    {
        public void Log(string message, TraceEventType eventType) { }
    }
}