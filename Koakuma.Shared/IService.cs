using Koakuma.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public interface IService : IModule
    {
        void Start();
        void Stop();

        IEnumerable<string> Hooks { get; }

        IEnumerable<string> Invokes { get; }

        void Invoke(string command, BaseMessage msg, byte[] payload = null);
    }
}
