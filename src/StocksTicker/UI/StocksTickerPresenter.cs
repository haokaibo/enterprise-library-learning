using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Timers;
using System.Diagnostics;
using StocksTicker.Loggers;
using StocksTicker.StockQuoteServices;
using PersistenceFramework;

namespace StocksTicker.UI
{
    public class StocksTickerPresenter
    {
        /// <summary>
        /// The constructor of the StocksTickerPresenter class. All the dependency component are passed thru the constructor.
        /// </summary>
        /// <param name="view">The view render</param>
        /// <param name="stockQuoteService">Business service</param>
        /// <param name="repository">Repository</param>
        public StocksTickerPresenter(
            IStocksTickerView view,
            IStockQuoteService stockQuoteService,
            IRepository<StockQuote> repository)
        {
            this.view = view;
            this.stockQuoteService = stockQuoteService;
            this.repository = repository;

            this.view.Subscribe += this.OnSubscribe;
            this.view.Unsubscribe += this.OnUnsubscribe;
            this.view.RefreshEnabledChanged += this.OnRefreshEnabledChanged;

            this.refreshTimer = new System.Timers.Timer();
            this.refreshTimer.AutoReset = false;
            this.refreshTimer.Elapsed += this.OnRefreshTimerElapsed;

            this.RefreshInterval = 5;   // default refresh interval

            this.logger = new NullLogger();
        }

        private ILogger logger;

        private int refreshInterval;
        public int RefreshInterval
        {
            get { return refreshInterval; }
            set
            {
                refreshInterval = value;
                // workaround from doc to avoid firing the event
                this.refreshTimer.AutoReset = true;
                this.refreshTimer.Interval = 1000d * value;
                this.refreshTimer.AutoReset = false;
            }
        }

        private void OnSubscribe(object source, EventArgs args)
        {
            string symbol = this.view.Symbol.ToUpper(CultureInfo.CurrentCulture);

            if (!symbolValidationRegex.IsMatch(symbol))
            {
                this.view.NotifyInvalidSymbol();
                return;
            }

            lock (quotesLock)
            {
                if (!this.quotes.ContainsKey(symbol))
                {
                    this.view.AddSymbol(symbol);
                    this.quotes.Add(symbol, null);
                }
            }
        }

        private void OnUnsubscribe(object source, EventArgs args)
        {
            IEnumerable<string> symbolsToRemove = this.view.SelectedSymbols;

            lock (quotesLock)
            {
                foreach (string symbolToRemove in symbolsToRemove)
                {
                    if (this.quotes.ContainsKey(symbolToRemove))
                    {
                        this.quotes.Remove(symbolToRemove);
                        this.view.RemoveSymbol(symbolToRemove);
                    }
                }
            }
        }

        private void OnRefreshEnabledChanged(object source, EventArgs args)
        {
            this.refreshTimer.Enabled = this.view.RefreshEnabled;
        }

        private void OnRefreshTimerElapsed(object source, ElapsedEventArgs args)
        {
            logger.Log(this.GetType() + ": Refresh timer elapsed", TraceEventType.Information);

            if (this.quotes.Count > 0)
            {
                this.RefreshQuotes(GetSymbols());
            }

            this.refreshTimer.Enabled = this.view.RefreshEnabled;
        }

        private void RefreshQuotes(List<string> symbols)
        {
            IDictionary<string, StockQuote> refreshedQuotes = null;

            // 1. Get the Symbols
            try
            {
                refreshedQuotes = this.stockQuoteService.GetQuotes(symbols);
            }
            catch (StockQuoteServiceException)
            {
                logger.Log(this.GetType().ToString() + ": Error invoking service", TraceEventType.Error);
                this.view.NotifyServiceStatus("Error invoking service");
                return;
            }
            catch (Exception)
            {
                logger.Log(this.GetType() + "Unknown error invoking service", TraceEventType.Error);
                this.view.NotifyServiceStatus("Unknown error invoking service");
                return;
            }

            // Update the Symbols' stock quotes.
            List<StockQuote> updatedQuotes = new List<StockQuote>();
            lock (quotesLock)
            {
                foreach (KeyValuePair<string, StockQuote> kvp in refreshedQuotes)
                {
                    StockQuote currentQuote;
                    StockQuote updatedQuote = kvp.Value;

                    if (this.quotes.TryGetValue(kvp.Key, out currentQuote))
                    {
                        if ((currentQuote == null && updatedQuote != null)
                            || (currentQuote != null
                                    && updatedQuote != null
                                    && currentQuote.LastUpdate < updatedQuote.LastUpdate))
                        {
                            this.quotes[kvp.Key] = updatedQuote;
                            updatedQuotes.Add(updatedQuote);

                            // Log logic
                            logger.Log(this.GetType().ToString() + ": StockQuote for " + kvp.Key + " was updated", TraceEventType.Information);

                            // Repository logic
                            SaveQuote(updatedQuote);
                        }
                        else
                        {
                            // Log logic
                            logger.Log(this.GetType().ToString() + ": StockQuote for " + kvp.Key + " was not updated", TraceEventType.Information);
                        }
                    }
                    else
                    {
                        // Log logic
                        logger.Log(this.GetType() + ": Received quote for unknown symbol " + kvp.Key, TraceEventType.Warning);
                    }
                }
            }

            if (updatedQuotes.Count > 0)
            {
                // Log logic
                logger.Log("Updates received, updating view", TraceEventType.Information);
                this.view.UpdateQuotes(updatedQuotes);
            }
        }

        private void SaveQuote(StockQuote updatedQuote)
        {
            try
            {
                this.repository.Save(updatedQuote);
            }
            catch (RepositoryException e)
            {
                // Log logic
                this.logger.Log(
                    string.Format(
                        this.GetType().ToString() + ": Error saving the updated quote for '{0}': {1}",
                        updatedQuote.Symbol,
                        e.Message),
                    TraceEventType.Warning);
            }
        }

        private List<string> GetSymbols()
        {
            List<string> symbols;

            lock (quotesLock)
            {
                symbols = new List<string>(this.quotes.Keys.Count);
                foreach (string symbol in this.quotes.Keys)
                {
                    symbols.Add(symbol);
                }
            }

            return symbols;
        }

        private static readonly Regex symbolValidationRegex = new Regex("^[a-zA-Z]+$");

        private readonly IStocksTickerView view;
        private readonly IStockQuoteService stockQuoteService;
        private readonly IRepository<StockQuote> repository;
        private readonly Dictionary<string, StockQuote> quotes = new Dictionary<string, StockQuote>();
        private readonly object quotesLock = new object();
        private readonly Timer refreshTimer;

        private StocksTicker.StockQuoteServices.IStockQuoteService StockQuoteService
        {
            get
            {
                return this.stockQuoteService;
            }
        }

        public StocksTicker.Loggers.ILogger Logger
        {
            get
            {
                return this.logger;
            }
            set
            {
                this.logger = value;
            }
        }

        public IStocksTickerView View
        {
            get
            {
                return view;
            }
        }
    }
}
