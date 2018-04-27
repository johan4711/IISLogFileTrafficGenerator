using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IISLogTrafficGenerator.Logic.Events;
using IISLogTrafficGenerator.Logic.Logging;

namespace IISLogTrafficGenerator.Logic.Runners
{
    public abstract class BaseRunner
    {
        public int UrlCount { get; private set; }
        public int Failedurls { get; private set; }
        public int Errors404 { get; private set; }
        public int Errors500 { get; private set; }
        public int ErrorsOther { get; private set; }
        public int NewUrls { get; set; }
        public int NoResponse { get; private set; }

        private bool dowaiting = true;
        //private int threadcount = 0;

        public ConcurrentBag<int> ThreadIds { get; } = new ConcurrentBag<int>();

        public ConcurrentDictionary<int, int> ErrorCodeCount = new ConcurrentDictionary<int, int>();
        public IOutputPipeline OutputPipeline { get; set; }

        public bool Progressing { get; set; }

        public static log4net.ILog Log { get; set; }

        protected BaseRunner()
        {
            Status = RunnerStatus.NotStarted;
        }

        public RunnerStatus Status { get; set; }

        public void OnClientDoneEvent(object sender)
        {
            Progressing = false;
        }
        public void OnClientErrorNoResponseEvent(object sender, ClientErrorNoResponseEventArgs clientErrorNoResponseEventArgs)
        {
            Log.Debug(clientErrorNoResponseEventArgs.InnerException);
            NoResponse++;
        }

        public void OnClientErrorResponseEvent(object sender, ClientErrorResponseEventArgs clientErrorResponseEventArgs)
        {
            var code = clientErrorResponseEventArgs.Code;
            if (!ErrorCodeCount.ContainsKey(code))
            {
                ErrorCodeCount.TryAdd(code, 0);
            }
            ErrorCodeCount[code]++;

            switch (clientErrorResponseEventArgs.Code)
            {
                case 404:
                    Errors404++;
                    break;
                case 500:
                    Errors500++;
                    break;
                default:
                    ErrorsOther++;
                    break;
            }
        }

        public void OnClientErrorOtherEvent(object sender, Exception innerException)
        {
            ErrorsOther++;
        }

        public void OnClientStoppedEvent(object sender, int threadId)
        {
            int idontcare = threadId;
            ThreadIds.TryTake(out idontcare);
            UrlCount++;
            NewUrls++;
            if (AbortThreads) Thread.CurrentThread.Abort();
        }

        public bool AbortThreads { get; set; } = true;

        public void OnClientStartedEvent(object sender, int threadId)
        {
            ThreadIds.Add(threadId);
        }

        public abstract void StartProgram(string pathToLog, string serverUrl, WaitMode waitMode, bool restart,
            IOutputPipeline output);
    }
}
