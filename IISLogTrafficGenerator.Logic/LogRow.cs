namespace IISLogTrafficGenerator.Logic
{
	using System;
	using System.Globalization;

	public class LogRow
	{
		public string Url { get; set; }
		public string Port { get; set; }
		public DateTime Time { get; set; }

		public LogRow(string url, string port, DateTime time)
        {
            this.Url = url;
			this.Time = time;
			this.Port = port;
        }

		public override bool Equals(object obj)
		{
			var otherRow = obj as LogRow;
			if (otherRow == null
				|| !otherRow.Port.Equals(this.Port)
				|| !otherRow.Time.Equals(this.Time)
				|| !otherRow.Url.Equals(this.Url)) return false;

			return true;
		}

		public override int GetHashCode()
		{
			var hash = string.Concat(this.Url, this.Port, this.Time.ToString(CultureInfo.InvariantCulture)).GetHashCode();
			return hash;
		}

		public override string ToString()
		{
			return string.Concat(this.Time, " ", this.Port, " ", this.Url);
		}
	}
}