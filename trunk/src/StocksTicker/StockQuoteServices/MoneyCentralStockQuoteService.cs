using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.XPath;
using StocksTicker.com.microsoft.office;
using StocksTicker.Loggers;
using System.Diagnostics;

namespace StocksTicker.StockQuoteServices
{
    public class MoneyCentralStockQuoteService : IStockQuoteService
    {
        private MoneyCentralRemote implementation;

        public MoneyCentralStockQuoteService()
            : this(new MoneyCentralRemote())
        { }

        public MoneyCentralStockQuoteService(MoneyCentralRemote implementation)
        {
            this.implementation = implementation;
            this.logger = new NullLogger();
        }

        private ILogger logger;

        #region StockQuoteService Members

        public IDictionary<string, StockQuote> GetQuotes(IEnumerable<string> symbols)
        {
            Dictionary<string, StockQuote> quotes = new Dictionary<string, StockQuote>();
            StringBuilder flattenedSymbols = new StringBuilder();
            foreach (string symbol in symbols)
            {
                quotes.Add(symbol, null);
                flattenedSymbols.Append(symbol);
                flattenedSymbols.Append(' ');
            }

            // Log logic
            logger.Log("Retrieving quotes for " + flattenedSymbols, TraceEventType.Information);

            string payload = null;

            try
            {
                payload = implementation.DDSQuery(flattenedSymbols.ToString(), null, null);
            }
            catch (Exception e)
            {
                string message = "Error invoking service: " + e.Message;
                logger.Log(message, TraceEventType.Error);
                throw new StockQuoteServiceException(message, e);
            }

            // Log logic
            logger.Log("Received result for " + flattenedSymbols, TraceEventType.Information);

            using (TextReader payloadReader = new StringReader(payload))
            {
                XPathDocument document = null;
                try
                {
                    document = new XPathDocument(payloadReader);
                }
                catch (Exception e)
                {
                    string message = "Error parsing XML payload: " + e.Message;
                    logger.Log(message, TraceEventType.Error);
                    throw new StockQuoteServiceException(message, e);
                }
                logger.Log("Parsed result for " + flattenedSymbols, TraceEventType.Information);

                XPathNavigator navigator = document.CreateNavigator();

                foreach (XPathNavigator quoteNavigator in navigator.Select("/StockData/Quotes/Quote"))
                {
                    StockQuote quote = BuildQuote(quoteNavigator);

                    if (quotes.ContainsKey(quote.Symbol))
                    {
                        quotes[quote.Symbol] = quote;
                    }
                    else
                    {
                        logger.Log("Unexpected symbol '" + quote.Symbol + "'", TraceEventType.Warning);
                    }
                }
            }

            return quotes;
        }

        #endregion

        private static StockQuote BuildQuote(XPathNavigator quoteNavigator)
        {
            if (!quoteNavigator.MoveToAttribute("Symbol", string.Empty))
            {
                // error
            }
            string symbol = quoteNavigator.Value;
            double last = 0d, volume = 0d, change = 0d, percentChange = 0d;
            string lastTimeString;
            DateTime lastUpdate = DateTime.Today;
            quoteNavigator.MoveToParent();
            if (quoteNavigator.MoveToChild("Last", string.Empty))
            {
                last = quoteNavigator.ValueAsDouble;
            }
            if (quoteNavigator.MoveToNext("LastTime", string.Empty))
            {
                lastTimeString = quoteNavigator.Value;
                DateTime.TryParseExact(
                        lastTimeString,
                        "h:mm tt",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out lastUpdate);
            }
            if (quoteNavigator.MoveToNext("Volume", string.Empty))
            {
                string volumeText = quoteNavigator.Value;
                if (!double.TryParse(volumeText, NumberStyles.Any, CultureInfo.InvariantCulture, out volume))
                {
                    volume = 0d;
                }
            }
            if (quoteNavigator.MoveToNext("Change", string.Empty))
            {
                change = quoteNavigator.ValueAsDouble;
            }
            if (quoteNavigator.MoveToNext("PercentChange", string.Empty))
            {
                percentChange = quoteNavigator.ValueAsDouble / 100;
            }

            StockQuote quote = new StockQuote(symbol, volume, last, lastUpdate, change, percentChange);
            return quote;
        }

        public StocksTicker.Loggers.ILogger Logger
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
    }
}
