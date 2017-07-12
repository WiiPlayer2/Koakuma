using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    class ModuleLogger : ILogger
    {
        public ModuleLogger(KoakumaNode node, IModule module)
        {
            Node = node;
            Module = module;
        }

        public KoakumaNode Node { get; private set; }

        public IModule Module { get; private set; }

        public void Log(LogLevel level, object obj)
        {
            Node.Logger.Log(level, Module.ID, obj);
        }

        public void Log(LogLevel level, string message)
        {
            Node.Logger.Log(level, Module.ID, message);
        }

        public void Log(LogLevel level, string source, object obj)
        {
            Node.Logger.Log(level, $"{Module.ID}/{source}", obj);
        }

        public void Log(LogLevel level, string source, string message)
        {
            Node.Logger.Log(level, $"{Module.ID}/{source}", message);
        }
    }
}
