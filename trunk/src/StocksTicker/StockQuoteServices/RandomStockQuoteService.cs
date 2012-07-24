using System;
using System.Collections.Generic;
using System.Text;
using StocksTicker.Loggers;
using System.Diagnostics;

namespace StocksTicker.StockQuoteServices
{
    public class RandomStockQuoteService : IStockQuoteService
    {
        private IDictionary<string, StockQuote> quotes = new Dictionary<string, StockQuote>();
        private Random random = new Random();

        public RandomStockQuoteService()
        {
            this.logger = new NullLogger();
        }

        private ILogger logger;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public IDictionary<string, StockQuote> GetQuotes(IEnumerable<string> symbols)
        {
            this.logger.Log(this.GetType() + ": Generating random quotes", TraceEventType.Information);

            foreach (string symbol in symbols)
            {
                if (!quotes.ContainsKey(symbol) || random.Next(4) == 0)
                {
                    quotes[symbol] = GenerateRandomQuote(symbol);
                }
            }

            return new Dictionary<string, StockQuote>(quotes);
        }

        private StockQuote GenerateRandomQuote(string symbol)
        {
            int changeMultiplier = random.Next(2) == 0 ? 1 : -1;

            return new StockQuote(
                    symbol,
                    (double)random.Next(100000000),
                    (double)random.Next(1000) / 4d,
                    DateTime.Now,
                    (double)random.Next(200) / 10d * changeMultiplier,
                    (double)random.Next(40) / 100d * changeMultiplier);

        }

    }
}
