using System;
using System.Collections.Generic;
using System.Text;
using PersistenceFramework;

namespace StocksTicker
{
    public class RandomStockQuoteValidator : IValidator<StockQuote>
    {
        private readonly Random random = new Random();

        public bool IsValid(StockQuote instance)
        {
            return random.Next(3) == 0; // one out of three will fail randomly
        }
    }
}