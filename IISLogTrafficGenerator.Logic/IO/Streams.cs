using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IISLogTrafficGenerator.Logic.IO
{
	public class Streams
	{
		public static Stream CopyStream(Stream inputStream)
		{
			const int readSize = 1024;
			byte[] buffer = new byte[readSize];
			MemoryStream ms = new MemoryStream();

			int count = inputStream.Read(buffer, 0, readSize);
			while (count > 0)
			{
				ms.Write(buffer, 0, count);
				count = inputStream.Read(buffer, 0, readSize);
			}
			ms.Position = 0;
			return ms;
		}
	}
}
