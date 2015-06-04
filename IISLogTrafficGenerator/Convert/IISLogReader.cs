using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace IISLogTrafficGenerator.Convert
{
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
            LogQueryClassClass logQuery = new LogQueryClassClass();
            COMIISW3CInputContextClassClass iisInputFormat = new COMIISW3CInputContextClassClass();

			string query = @"SELECT time, cs-method, cs-uri-stem, cs-uri-query, s-port, c-ip FROM " + iisLogPath;

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