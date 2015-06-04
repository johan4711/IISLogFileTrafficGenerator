using System;

namespace IISLogTrafficGenerator.Logic.Events
{
	public class ConfigurationErrorEventArgs
	{
		public ConfigurationErrorEventArgs(string s) { Text = s; }
        public String Text {get; private set;} // readonly
    }
}
