using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogTrafficGenerator.Tests
{
	using System.IO;

	using IISLogTrafficGenerator.Logic;

	using NUnit.Framework;

	[TestFixture]
    public class TestLogReader
    {

		public MemoryStream Setup()
		{

			var logrows = @"#Software: Microsoft Internet Information Services 8.5
#Version: 1.0
#Date: 2014-09-18 09:20:54
#Fields: date time s-ip cs-method cs-uri-stem cs-uri-query s-port cs-username c-ip cs(User-Agent) cs(Referer) sc-status sc-substatus sc-win32-status time-taken
2014-09-18 09:20:54 127.0.0.1 GET /episerver/cms/ - 80 user 127.0.0.1 Mozilla/5.0+(Windows+NT+6.3;+WOW64;+rv:32.0)+Gecko/20100101+Firefox/32.0 - 200 0 0 4921
2014-09-18 09:20:54 127.0.0.1 GET /episerver/Shell/7.13.0.0/ClientResources/epi/themes/legacy/ShellCore.css - 80 user 127.0.0.1 Mozilla/5.0+(Windows+NT+6.3;+WOW64;+rv:32.0)+Gecko/20100101+Firefox/32.0 http://alloy75.local/episerver/cms/ 200 0 0 32
2014-09-18 09:20:55 127.0.0.1 GET /episerver/Shell/7.13.0.0/ClientResources/epi/themes/legacy/DojoDashboardCompatibility.css - 80 user 127.0.0.1 Mozilla/5.0+(Windows+NT+6.3;+WOW64;+rv:32.0)+Gecko/20100101+Firefox/32.0 http://alloy75.local/episerver/cms/ 200 0 0 3
";

			var ms = new MemoryStream(Encoding.UTF8.GetBytes(logrows));

			return ms;
		}

		[Test]
		public void ParseLogFile_Should_Return_An_IEnumerable_With_LogRow_Objects()
		{
			LogReader lr = new LogReader(this.Setup());
			var result = lr.ParseLogFile().ToList();

			// test 1
			int actualcount = result.Count();
			int expectedcount = 3;

			Assert.AreEqual(expectedcount, actualcount);
		}

		[Test]
		public void LogFileRow_Should_Contain_A_Correctly_Parsed_Result()
		{
			LogReader lr = new LogReader(this.Setup());

			var result = lr.ParseLogFile().ToList();

			// test 1

			var expectedLogRow1 = new LogRow("/episerver/cms/", "80", new DateTime(2014,9,18,9,20,54));
			var actualLogRow = result[0];

			Assert.AreEqual(expectedLogRow1.Port, actualLogRow.Port);
			Assert.AreEqual(expectedLogRow1.Url, actualLogRow.Url);
			Assert.AreEqual(expectedLogRow1.Time, actualLogRow.Time);

			var expectedLogRow2 = new LogRow("/episerver/Shell/7.13.0.0/ClientResources/epi/themes/legacy/ShellCore.css", "80", new DateTime(2014, 9, 18, 9, 20, 54));
			actualLogRow = result[1];

			Assert.AreEqual(expectedLogRow2.Port, actualLogRow.Port);
			Assert.AreEqual(expectedLogRow2.Url, actualLogRow.Url);
			Assert.AreEqual(expectedLogRow2.Time, actualLogRow.Time);

			var expectedLogRow3 = new LogRow("/episerver/Shell/7.13.0.0/ClientResources/epi/themes/legacy/DojoDashboardCompatibility.css", "80", new DateTime(2014, 9, 18, 9, 20, 55));
			actualLogRow = result[2];

			Assert.AreEqual(expectedLogRow3.Port, actualLogRow.Port);
			Assert.AreEqual(expectedLogRow3.Url, actualLogRow.Url);
			Assert.AreEqual(expectedLogRow3.Time, actualLogRow.Time);
		}
    }
}
