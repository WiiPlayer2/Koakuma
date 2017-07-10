using Koakuma.Shared.Messages;
using System.Collections.Generic;

namespace Koakuma.Shared
{
    public interface IService : IModule
    {
        void Start();

        void Stop();

        IEnumerable<string> Hooks { get; }

        IEnumerable<string> Invokes { get; }

        BaseMessage Invoke(ModuleID from, string command, byte[] payload = null);
    }
}