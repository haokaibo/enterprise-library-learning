using System;
using System.Runtime.Serialization;

namespace StocksTicker.StockQuoteServices
{
    public class StockQuoteServiceException : Exception
    {
        public StockQuoteServiceException()
            : base("Stock quote service error")
        {
        }

        public StockQuoteServiceException(string message)
            : base(message)
        {
        }

        protected StockQuoteServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public StockQuoteServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}