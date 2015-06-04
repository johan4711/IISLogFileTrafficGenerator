using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using IISLogTrafficGenerator.Convert;
using IISLogTrafficGenerator.Logic;
using IISLogTrafficGenerator.Logic.Events;
using log4net.Config;

namespace IISLogTrafficGenerator
{
    class Program
    {
		
		private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private int urlcount = 0;
		private int failedurls = 0;
    	private int errors404 = 0;
    	private int errors500 = 0;
    	private int errorsother = 0;
		private int newurls = 0;
		private bool dowaiting = true;
		private int threadcount = 0;
	    private int noresponse = 0;

		private bool progressing = false;
		private ConcurrentDictionary<int, int> ErrorCodeCount = new ConcurrentDictionary<int, int>();

    	static void Main(string[] args)
        {

            //args
			string pathToLog = ParseFileName(args); // @"C:\tmp\vikingline\loggar\logs\u_ex130106.log");
			string serverUrl = ParseServerUrl(args); //"http://vikingline.market.se.local{0}");	
			var dowaiting = ParseDoWait(args);
    		var restart = ParseRestartWhenFinished(args);

			if (ParseDisplayHelp(args)) 
        	{
        		DisplayHelp();
        	}
            new Program().StartProgram(pathToLog, serverUrl, dowaiting, restart);
        }

		private void StartProgram(string pathToLog, string serverUrl, WaitMode b, bool restart)
		{
			ThreadPool.SetMinThreads(10, 4);
			System.Net.ServicePointManager.DefaultConnectionLimit = 100;

			int count = 0;
			progressing = true;
			var d = new Logic.Downloader(pathToLog, serverUrl, b, restart);
			
			d.ClientStartedEvent += OnClientStartedEvent;
			d.ClientStoppedEvent += OnClientStoppedEvent;
			d.ClientErrorOtherEvent += OnClientErrorOtherEvent;
			d.ClientErrorResponseEvent += OnClientErrorResponseEvent;
			d.ClientErrorNoResponseEvent += OnClientErrorNoResponseEvent;
			d.ClientDoneEvent += OnClientDoneEvent;
			Thread threadStart = new Thread(d.CreateRequests);
			threadStart.Start();
			
			TimeSpan lastTimespan = new TimeSpan(0, 0, 0, 0);
			int waitinterval = 1;
			do
			{
				Console.WriteLine(String.Format("{0}, urls: {1} ({2}) fail: {3} 404: {4} 500: {5}, o: {6}, nr: {7}, t: {8}", lastTimespan, newurls, urlcount, failedurls, errors404, errors500, errorsother, noresponse, threadcount));
				if (count == 10)
				{
					DumpErrorCodeCount();
					count = 0;
				}
				count++;
				newurls = 0;
				lastTimespan = lastTimespan.Add(new TimeSpan(0, 0, waitinterval));
				Thread.Sleep(waitinterval * 1000);
			} while (progressing);
    	}

	    private void OnClientErrorNoResponseEvent(object sender, ClientErrorNoResponseEventArgs clientErrorNoResponseEventArgs)
	    {
			Log.Debug(clientErrorNoResponseEventArgs.InnerException);
		    noresponse++;
	    }

	    private void DumpErrorCodeCount()
	    {
		    StringBuilder result = new StringBuilder();
			foreach (var kvp in ErrorCodeCount)
			{
				result.AppendFormat(" {0}:{1} ", kvp.Key, kvp.Value);
			}
			Console.WriteLine(result.ToString());
	    }

		private void OnClientDoneEvent(object sender)
    	{
			progressing = false;
    	}

    	private void OnClientErrorResponseEvent(object sender, ClientErrorResponseEventArgs clientErrorResponseEventArgs)
    	{
    		var code = clientErrorResponseEventArgs.Code;
			if (!ErrorCodeCount.ContainsKey(code))
			{
				ErrorCodeCount.TryAdd(code, 0);
			}
    		ErrorCodeCount[code]++;

    		switch (clientErrorResponseEventArgs.Code)
    		{
				case 404: errors404++;
					break;
				case 500: errors500++;
					break;
				default: errorsother++;
					break;
    		}
    	}

    	private void OnClientErrorOtherEvent(object sender, Exception innerException)
    	{
			threadcount--;
			errorsother++;
    	}

    	private void OnClientStoppedEvent(object sender)
    	{
			threadcount--;
			urlcount++;
			newurls++;
    	}

    	private void OnClientStartedEvent(object sender)
    	{
			threadcount++;
    	}


    	private static void DisplayHelp()
    	{
			DisplayHeader();
			DisplayLogfileOptionHelp();
			DisplayServerUrlOptionHelp();
			DisplayNowaitOptionHelp();
			DisplayRestartFromTopOptionHelp();
    		DisplayStaticWaitOptionHelp();
			Environment.Exit(1);
    	}

	    private static void DisplayStaticWaitOptionHelp()
	    {
			Console.WriteLine("/ws <milliseconds> waits a specified amount of milliseconds between each request. Default is 100");
	    }

	    private static void DisplayHeader()
    	{
			Console.WriteLine("IISLogTrafficGenerator 0.1\r\n==========================\r\n\r\nParses n IIS log file and generates http traffic from this\r\n");
    	}

    	private static void DisplayNowaitOptionHelp()
    	{
			Console.WriteLine("/w disables the waiting between requests. Normally the parser checks the timestamp of the log message and tries to mimic the request flow by waiting the difference in second but by specifying this option, it wil run everything as it reads it. Beware, this might build up a large swarm of traffic");
    	}

    	private void ThrowLogfileOptionMissingError()
		{
			DisplayLogfileOptionHelp();
			throw new ApplicationException("You need to specify logfile name");
		}

		private static void DisplayLogfileOptionHelp()
		{
			Console.WriteLine("/f <path and name of logfile>, eg, /f c:\\inetpub\\logs\\w3svc2\\ex_130101.log");
		}

		private static void DisplayServerUrlOptionHelp()
		{
			Console.WriteLine(@"/s <server base url>, eg, /s ""http://mysite.com"". If omitted, http://localhost will be used");
		}

	    private static void DisplayRestartFromTopOptionHelp()
	    {
			Console.WriteLine(@"/r restart from top of log file when run is complete, ie, will run contínously until interrupted");
	    }
		private static void DisplayUnableToParseTimespanMessage(string startTime)
		{
			Console.WriteLine("Unable to parse timespan " + startTime);
		}

    	public static TimeSpan ParseStartTime(string[] args)
		{
			TimeSpan tsSpan = new TimeSpan(0,0,0);
			int index = Array.FindIndex(args, p => p.ToLower().Equals("/f"));
			if (index > -1)
			{
				string startTime = args[index + 1];
				if (!TimeSpan.TryParse(startTime, out tsSpan))
				{
					DisplayUnableToParseTimespanMessage(startTime);
				}
			}

			return tsSpan;
		}

    	public static string ParseFileName(string[] args)
		{
			string fileName = String.Empty;
			int index = Array.FindIndex(args, p => p.ToLower().Equals("/f"));
			if (index > -1)
			{
				fileName = args[index + 1];
			}

			return fileName;
		}

		public static string ParseServerUrl(string[] args)
		{
			string url = String.Empty;
			int index = Array.FindIndex(args, p => p.ToLower().Equals("/s"));
			if (index > -1)
			{
				url = args[index + 1];
			}

			return url;
		}

		public static bool ParseRestartWhenFinished(string[] args)
		{
			int index = Array.FindIndex(args, p => p.ToLower().Equals("/r"));
			if (index > -1)
			{
				return true;
			}

			return false;
		}

		public static WaitMode ParseDoWait(string[] args)
		{
			string url = String.Empty;
			int index = Array.FindIndex(args, p => p.ToLower().Equals("/w"));
			if (index > -1)
			{
				return WaitMode.None;
			}
			index = Array.FindIndex(args, p => p.ToLower().Equals("/ws"));
			if (index > -1)
			{
				if ((index + 1) < args.Length)
				{
					// not currently implemented
					// parse next value, send it to Downloader
				}
				return WaitMode.Static;
			}

			return WaitMode.LogInterval;
		}

		public static bool ParseDisplayHelp(string[] args)
		{
			int index = Array.FindIndex(args, p => p.ToLower().Equals("/?"));
			if (index > -1 || args.Length == 0)
			{
				return true;
			}

			return false;
		}

   
    }
}