using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace StocksTicker.Loggers
{
    public class CompositeLogger : ILogger
    {
        private IEnumerable<ILogger> loggers;

        public CompositeLogger(ILogger[] loggers)
        {
            this.loggers = (IEnumerable<ILogger>)loggers.Clone();
        }

        public void Log(string message, TraceEventType eventType)
        {
            foreach (ILogger logger in this.loggers)
            {
                logger.Log(message, eventType);
            }
        }
    }
}