using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    class NullLogger : ILogger
    {
        public void Log(LogLevel level, object obj) { }

        public void Log(LogLevel level, string message) { }

        public void Log(LogLevel level, string source, object obj) { }

        public void Log(LogLevel level, string source, string message) { }
    }
}
