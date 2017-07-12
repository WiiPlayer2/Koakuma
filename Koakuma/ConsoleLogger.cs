using Koakuma.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma
{
    class ConsoleLogger : ILogger
    {
        public ConsoleLogger()
        {
            OutputLevel = LogLevel.Debug;
        }

        public LogLevel OutputLevel { get; set; }

        public void Log(LogLevel level, object obj)
        {
            Log(level, null, obj);
        }

        public void Log(LogLevel level, string message)
        {
            Log(level, null, message);
        }

        public void Log(LogLevel level, string source, object obj)
        {
            if (obj != null)
            {
                Log(level, source, $"<0x{obj.GetHashCode().ToString("X8")}> {obj}");
            }
            else
            {
                Log(level, source, "<null>");
            }
        }

        public void Log(LogLevel level, string source, string message)
        {
            if (level >= OutputLevel)
            {
                lock (Console.Out)
                {
                    if (message.Contains('\n'))
                    {
                        foreach (var m in message.Split('\n'))
                        {
                            Log(level, source, m);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{source,10}] {level.ToString()[0]}: {message}");
                    }
                }
            }
        }
    }
}
