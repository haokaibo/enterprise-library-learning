using System;
using System.Collections.Generic;

namespace StocksTicker.UI
{
    public interface IStocksTickerView
    {
        void AddSymbol(string symbol);
        void NotifyInvalidSymbol();
        void NotifyServiceStatus(string status);
        void RemoveSymbol(string symbol);
        void UpdateQuotes(IEnumerable<StockQuote> quotesToUpdate);

        bool RefreshEnabled { get; }
        IEnumerable<string> SelectedSymbols { get; }
        string Symbol { get; }

        event EventHandler<EventArgs> RefreshEnabledChanged;
        event EventHandler<EventArgs> Subscribe;
        event EventHandler<EventArgs> Unsubscribe;
    }
}
