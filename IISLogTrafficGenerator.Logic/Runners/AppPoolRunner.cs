using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IISLogTrafficGenerator.Logic.Logging;

namespace IISLogTrafficGenerator.Logic.Runners
{
    public class AppPoolRunner : BaseRunner
    {
        public AppPoolRunner()
        {
            AbortThreads = false;
        }

        public override void StartProgram(string pathToLog, string serverUrl, WaitMode waitMode, bool restart, IOutputPipeline output)
        {
            Status = RunnerStatus.Started;
            OutputPipeline = output;

            ThreadPool.SetMinThreads(10, 4);
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;

            int count = 0;
            Progressing = true;
            var d = new Logic.Downloader(pathToLog, serverUrl, waitMode, restart);

            d.ClientStartedEvent += OnClientStartedEvent;
            d.ClientStoppedEvent += OnClientStoppedEvent;
            d.ClientErrorOtherEvent += OnClientErrorOtherEvent;
            d.ClientErrorResponseEvent += OnClientErrorResponseEvent;
            d.ClientErrorNoResponseEvent += OnClientErrorNoResponseEvent;
            d.ClientDoneEvent += OnClientDoneEvent;
            Thread threadStart = new Thread(d.CreateRequests);
            threadStart.Start();

            Status = RunnerStatus.Running;

            //TimeSpan lastTimespan = new TimeSpan(0, 0, 0, 0);
            //int waitinterval = 1;
            //do
            //{
            //    OutputPipeline.Write($"{lastTimespan}, urls: {NewUrls} ({UrlCount}) fail: {Failedurls} 404: {Errors404} 500: {Errors500}, o: {ErrorsOther}, nr: {NoResponse}, t: {ThreadIds.Count}");
            //    if (count == 10)
            //    {
            //        DumpErrorCodeCount();
            //        count = 0;
            //    }
            //    count++;
            //    NewUrls = 0;
            //    lastTimespan = lastTimespan.Add(new TimeSpan(0, 0, waitinterval));
            //    Thread.Sleep(waitinterval * 1000);
            //} while (Progressing);

            //Status = RunnerStatus.Done;
        }

        public void CheckStatus()
        {

        }
    }
}
