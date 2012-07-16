using System;
using System.Collections.Generic;
using System.Text;

namespace StocksTicker.StockQuoteServices
{
    public interface IStockQuoteService
    {
        IDictionary<string, StockQuote> GetQuotes(IEnumerable<string> symbol);
    }
}
