using System.Diagnostics;

namespace StocksTicker.Loggers
{
    public interface ILogger
    {
        void Log(string message, TraceEventType eventType);
    }
}
