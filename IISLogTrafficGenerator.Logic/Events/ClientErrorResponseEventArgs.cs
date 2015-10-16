using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IISLogTrafficGenerator.Logic.Events
{
	public class ClientErrorResponseEventArgs
	{
		public ClientErrorResponseEventArgs(int code) { Code = code; }
        public int Code {get; private set;} 
	}
}
