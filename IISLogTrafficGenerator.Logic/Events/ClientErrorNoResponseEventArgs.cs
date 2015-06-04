using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IISLogTrafficGenerator.Logic.Events
{
	public class ClientErrorNoResponseEventArgs
	{
		public ClientErrorNoResponseEventArgs(Exception exception) { InnerException = exception; }
		public Exception InnerException { get; private set; } // readonly
	}
}
