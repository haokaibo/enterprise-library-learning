using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

using PersistenceFramework;
using StocksTicker.Loggers;
using StocksTicker.StockQuoteServices;
using StocksTicker.UI;
using System.Configuration.Install;
using System.Collections;
using StocksTicker.PerformanceCounter;
using System.Threading;

namespace StocksTicker
{
    static class Program
    {
        private static void InstallCounters()
        {
            //PerformanceCountersInstaller installer = new PerformanceCountersInstaller(new SystemConfigurationSource());
            //IDictionary state = new System.Collections.Hashtable();
            //installer.Context = new InstallContext();
            //installer.Install(state);
            //installer.Commit(state);
            //Console.WriteLine("Performance counters have been successfully installed. ");
        }
        private static void RemoveCounters()
        {
            //PerformanceCountersInstaller installer = new PerformanceCountersInstaller(new SystemConfigurationSource());
            //installer.Context = new InstallContext();
            //installer.Uninstall(null);
            //Console.WriteLine("Performance counters have been successfully removed. ");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            InstallCounters();
            Application.ThreadExit += new EventHandler(Application_ThreadExit);

            StocksTickerPresenter presenter = null;

            Console.WriteLine("Please choose the implementation mode:{0}1. Traditional mode {0}2. Unity dependency injection and policy injection"
                , Environment.NewLine);
            string strMode = Console.ReadLine();
            switch (strMode)
            {
                case "1":
                    #region Traditional Implementation
                    presenter = ImplementWithTranditionalMode(presenter);
                    #endregion
                    break;
                case "2":
                    #region Unity Implementation
                    presenter = ImplementWithUnityMode(presenter);
                    #endregion
                    break;
                default:
                    break;
            }
        }
        private static StocksTickerPresenter ImplementWithTranditionalMode(StocksTickerPresenter presenter)
        {
            RandomStockQuoteService stockQuoteService = new RandomStockQuoteService();
            stockQuoteService.Logger = new ConsoleLogger();

            presenter = new StocksTickerPresenter(
            new StocksTickerForm(),
            stockQuoteService,
            new DebugRepository<StockQuote>());

            presenter.Logger = new TraceSourceLogger("UI");

            Application.Run((Form)presenter.View);
            return presenter;
        }

        private static StocksTickerPresenter ImplementWithUnityMode(StocksTickerPresenter presenter)
        {
            using (IUnityContainer container = new UnityContainer())
            {
                container.LoadConfiguration();

                presenter = container.Resolve<StocksTickerPresenter>();
                Application.Run((Form)presenter.View);
            }
            return presenter;
        }



        static void Application_ThreadExit(object sender, EventArgs e)
        {
            RemoveCounters();
        }
    }
}