using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public interface ILogger
    {
        void Log(LogLevel level, string message);

        void Log(LogLevel level, object obj);

        void Log(LogLevel level, string source, string message);

        void Log(LogLevel level, string source, object obj);
    }
}
