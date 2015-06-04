namespace IISLogTrafficGenerator.Logic.Convert
{
	using System;
	using System.Collections.Generic;

	using IISLogTrafficGenerator.Convert;

	public class Converter
    {
        public IEnumerable<LogRow> GetRequestEnumerator(string pathToLogFile)
        {
            if (pathToLogFile == null) throw new ArgumentNullException("pathToLogFile");

            var reader = new LogReader(pathToLogFile);

            return reader.GetRequests();
          
        }
    }
}