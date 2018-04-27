using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogTrafficGenerator.Logic.Logging
{
    public class StringBuilderLogger: IOutputPipeline
    {
        private StringBuilder buffer = new StringBuilder();
        public void Write(string message)
        {
            buffer.Append(message);
        }

        public override string ToString()
        {
            return buffer.ToString();
        }
    }
}
