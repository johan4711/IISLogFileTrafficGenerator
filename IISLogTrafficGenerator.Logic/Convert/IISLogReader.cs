namespace IISLogTrafficGenerator.Logic.Convert
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	using IISLogTrafficGenerator.Convert;

	using MSUtil;

	public class IISLogReader
    {
        private string iisLogPath;

        public IISLogReader(string iisLogPath) 
        {
			this.iisLogPath = iisLogPath;
        }


        public IEnumerable<LogRow> GetRequests()
        {
            LogQueryClass logQuery = new LogQueryClass();
            COMIISW3CInputContextClass iisInputFormat = new COMIISW3CInputContextClass();

			string query = @"SELECT time, cs-method, cs-uri-stem, cs-uri-query, s-port, c-ip FROM " + this.iisLogPath;

            ILogRecordset recordSet = logQuery.Execute(query, iisInputFormat);
            while (!recordSet.atEnd())
            {
                ILogRecord record = recordSet.getRecord();
                if (record.getValueEx("cs-method").ToString() == "GET")
                {
                    string path = record.getValueEx("cs-uri-stem").ToString();
                    string querystring = record.getValueEx("cs-uri-query").ToString();

                    var urlBuilder = new StringBuilder();
                    urlBuilder.Append(path);
                    if (!String.IsNullOrEmpty(querystring))
                    {
                        urlBuilder.Append("?");
                        urlBuilder.Append(querystring);
                    }
					string ip = record.getValueEx("c-ip").ToString();
					var time = (DateTime)record.getValue("time");
                    var request = new LogRow(urlBuilder.ToString(), ip, time);
                    yield return request;
                }

                recordSet.moveNext();
            }

            recordSet.close();
        }
    }
}