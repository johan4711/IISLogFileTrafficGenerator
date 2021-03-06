﻿using System;
using System.IO;
using System.Net;
using System.Threading;

using IISLogTrafficGenerator.Logic.Events;
using log4net.Config;

namespace IISLogTrafficGenerator.Logic
{
	public class Downloader
	{
		private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		WaitMode dowaiting;
		private string logFile = string.Empty;
		private string targetServerUrl = string.Empty;
		private int staticWaitInterval = 100;

		private bool restartwhenfinished;

		// delegates and events
		public delegate void ConfigurationErrorEventHandler(object sender, ConfigurationErrorEventArgs e);
		public event ConfigurationErrorEventHandler LogFileOptionMissingEvent;

		public delegate void ClientStartedEventHandler(object sender, int threadId);
		public event ClientStartedEventHandler ClientStartedEvent;

		public delegate void ClientStoppedEventHandler(object sender, int threadId);
		public event ClientStoppedEventHandler ClientStoppedEvent;

		public delegate void ClientErrorResponseEventHandler(object sender, ClientErrorResponseEventArgs e);
		public event ClientErrorResponseEventHandler ClientErrorResponseEvent;

		public delegate void ClientErrorNoResponseEventHandler(object sender, ClientErrorNoResponseEventArgs e);
		public event ClientErrorNoResponseEventHandler ClientErrorNoResponseEvent;

		public delegate void ClientErrorOtherEventHandler(object sender, Exception innerException);
		public event ClientErrorOtherEventHandler ClientErrorOtherEvent;

		public delegate void ClientDoneEventHandler (object sender);
		public event ClientDoneEventHandler ClientDoneEvent;

		public int StaticWaitInterval
		{
			get { return staticWaitInterval; }
			set { staticWaitInterval = value; }
		}

		public void OnClientDoneEvent()
		{
			ClientDoneEventHandler handler = ClientDoneEvent;
			if (handler != null) handler(this);
		}

		public void OnClientErrorOtherEvent(Exception innerException)
		{
			ClientErrorOtherEventHandler handler = ClientErrorOtherEvent;
			if (handler != null) handler(this, innerException);
		}

		public void OnClientErrorResponseEvent(int code)
		{
			var handler = ClientErrorResponseEvent;
			if (handler != null) handler(this, new ClientErrorResponseEventArgs(code));
		}

		public void OnClientErrorNoResponseEvent(Exception ex)
		{
			var handler = ClientErrorNoResponseEvent;
			if (handler != null) handler(this, new ClientErrorNoResponseEventArgs(ex));
		}

		

		protected void OnClientStartedEvent()
		{
			ClientStartedEventHandler handler = ClientStartedEvent;
			if (handler != null) handler(this, Thread.CurrentThread.ManagedThreadId);
		}

		protected void OnClientStoppedEvent()
		{
			ClientStoppedEventHandler handler = ClientStoppedEvent;
			if (handler != null) handler(this, Thread.CurrentThread.ManagedThreadId);
		}

		protected void OnNoLogfileToProcess(string message)
		{
			// Raise the event by using the () operator.
			if (LogFileOptionMissingEvent != null)
				LogFileOptionMissingEvent(this, new ConfigurationErrorEventArgs(message));

		}

		public Downloader(string log, string serverUrl, WaitMode wait, bool restart)
		{
			logFile = log;
			targetServerUrl = serverUrl;
			dowaiting = wait;
			restartwhenfinished = restart;
		}

		public void CreateRequests()
		{
			if (String.IsNullOrEmpty(logFile))
			{
				OnNoLogfileToProcess("Logfile cannot be null or empty");
			}

			if (String.IsNullOrEmpty(targetServerUrl))
			{
				targetServerUrl = "http://localhost";
			}

			if (!targetServerUrl.StartsWith("http"))
			{
				targetServerUrl = "http://" + targetServerUrl;
			}

			if (targetServerUrl.EndsWith("/"))
			{
				targetServerUrl = targetServerUrl.Substring(0, targetServerUrl.Length - 1);
			}

			XmlConfigurator.Configure(new FileInfo("log.config"));

			var reader = new LogReader(logFile);

			var requests = reader.ParseLogFile();
			var lastTimespan = new TimeSpan(0, 0, 0);
			WebClientThreadStatus client = null;

			do
			{
				foreach (var request in requests)
				{
					if (request.Port != "80") request.Port = ":" + request.Port;
					else request.Port = "";
					string url = string.Format("{0}{1}{2}", targetServerUrl, request.Port, request.Url);

					if (dowaiting == WaitMode.LogInterval)
					{
						WaitTimespanDifference(lastTimespan, request.Time);
					}
					else if (dowaiting == WaitMode.Static)
					{
						Thread.Sleep(new TimeSpan(0, 0, 0, 0, StaticWaitInterval));
					}

					StartDownloadThread(url);

					lastTimespan = request.Time.TimeOfDay;
				}
			}
			while (restartwhenfinished);

			OnClientDoneEvent();
		}

		private void StartDownloadThread(string url)
		{
			Thread ts = new Thread(Download);
			ts.Start(url);
		}

		private void WaitTimespanDifference(TimeSpan lastTimespan, DateTime time)
		{
			var newTs = time.TimeOfDay.Subtract(lastTimespan);
			Thread.Sleep(newTs);
		}

		private void Download(object urlParameter)
		{
			var url = urlParameter as string;
			if (String.IsNullOrEmpty(url)) return;

			var parameters = new WebClientThreadStatus(WebRequest.Create(url), url);

			var request = parameters.Client;
			request.Credentials = CredentialCache.DefaultCredentials;
			
			StreamReader reader = null;
			Stream dataStream = null;
			HttpWebResponse response = null;

			while (!parameters.CheckStatus())
			{
				Thread.Sleep(50);
			}

			OnClientStartedEvent();

			try
			{
				parameters.StartDownload();
				response = (HttpWebResponse)request.GetResponse();
				dataStream = IO.Streams.CopyStream(response.GetResponseStream());
				reader = new StreamReader(dataStream);
				string responseFromServer = reader.ReadToEnd();
			}
			catch (WebException ex)
			{
				int code = 0;
				if (ex.Response != null)
				{
					code = (int)((HttpWebResponse)ex.Response).StatusCode;
					OnClientErrorResponseEvent(code);
				} else
				{
					OnClientErrorNoResponseEvent(ex);
				}

				Log.ErrorFormat("{0} {1}", code, parameters.Url);
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("other: {0}", parameters.Url);
				OnClientErrorOtherEvent(ex);
			}
			finally
			{
				reader?.Close();
				dataStream?.Close();
				response?.Close();
				parameters.DownloadFinished();
				OnClientStoppedEvent();
			}

		}
	}
}
