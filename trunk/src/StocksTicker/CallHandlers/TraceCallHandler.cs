using System;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.Practices.Unity.Utility;

namespace StocksTicker.CallHandlers
{
    class TraceCallHandler : ICallHandler, IDisposable
    {
        private TraceSource source;

        public TraceCallHandler(TraceSource source)
        {
            Guard.ArgumentNotNull(source, "source");
            this.source = source;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            #region Instrumentation: Timing
            Stopwatch watch = new Stopwatch();
            watch.Start();
            this.source.TraceInformation(
                "Invoking {0}",
                input.MethodBase.ToString());
            Console.WriteLine(getNext().Method.ToString());
            IMethodReturn methodReturn = getNext().Invoke(input, getNext);
            watch.Stop();

            // exception handling
            if (methodReturn.Exception == null)
            {
                // logging successful invoking
                this.source.TraceInformation(
                    "Successfully finished {1}{0}elapsed time:{2}ms",
                    Environment.NewLine,
                    input.MethodBase.ToString(),
                    watch.ElapsedMilliseconds.ToString("N0"));
            }
            else
            {
                // logging exception
                this.source.TraceInformation(
                    "Finished {0} with exception {1}: {2}",
                    input.MethodBase.ToString(),
                    methodReturn.Exception.GetType().Name,
                    methodReturn.Exception.Message);
            }

            this.source.Flush();

            #endregion
            return methodReturn;
        }

        private int order;
        public int Order
        {
            get
            {
                return order;
            }
            set
            {
                order = value;
            }
        }

        public void Dispose()
        {
            this.source.Close();
        }
    }

    class TraceCallHandlerAttribute : HandlerAttribute
    {
        private string sourceName;

        public TraceCallHandlerAttribute(string sourceName)
        {
            Guard.ArgumentNotNull(sourceName, "sourceName");
            this.sourceName = sourceName;
        }

        public override ICallHandler CreateHandler(IUnityContainer container)
        {
            return new TraceCallHandler(new TraceSource(this.sourceName));
        }
    }
}
