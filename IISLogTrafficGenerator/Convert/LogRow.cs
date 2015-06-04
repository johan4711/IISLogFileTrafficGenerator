using System;

namespace IISLogTrafficGenerator.Convert
{
	public class LogRow
	{
		public string Url { get; set; }
		public DateTime Time { get; set; }
		public string Ip { get; set; }

		public LogRow(string url, string ip, DateTime time)
        {
            Url = url;
			Ip = ip;
			Time = time;
        }
    }
}