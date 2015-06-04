using System;
using System.Net;

namespace IISLogTrafficGenerator.Logic
{
	public class WebClientThreadStatus: IDisposable
	{
		public HttpWebRequest Client;
		public string Url;
		private ThreadStatus status;
		private static object dlhandle = new object();
		private int id = 0;
		private static int STATICOBJECTCOUNTER = 0;

		public WebClientThreadStatus(WebRequest client, string url)
		{
			Client = (HttpWebRequest)client;
			Client.KeepAlive = false;

			Url = url;
			status = 0;
			id = STATICOBJECTCOUNTER + 1;
			STATICOBJECTCOUNTER++;
		}

		public ThreadStatus Status
		{
			get
			{
				lock (dlhandle)
				{
					return status;
				}
			}
		}

		public int ID
		{
			get { return id; }
		}

		public void StartDownload()
		{
			lock (dlhandle)
			{
				status = ThreadStatus.Downloading; 
			}
		}

		public void DownloadFinished()
		{
			lock (dlhandle)
			{
				status = ThreadStatus.Finished;
			}
		}

		public bool CheckStatus()
		{
			lock (dlhandle)
			{
				if (status == ThreadStatus.Downloading)
				{
					return false;
				}
				if (status == ThreadStatus.Idle)
				{
					status = ThreadStatus.Downloading;
					return true;
				}
				status = ThreadStatus.Finished;
				return true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{

		}
	}
}