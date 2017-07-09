using Koakuma.Shared.Messages;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public interface IKoakuma
    {
        ModuleID ModuleID { get; }

        void SendMessage(ModuleID receiver, BaseMessage msg, byte[] payload = null);
        void SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, Action<BaseMessage, byte[]> callback, Action timeoutCallback, byte[] payload = null);
        Task<BaseMessage> SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, byte[] payload = null);

        void Invoke(ModuleID receiver, string command, byte[] payload = null);
        void Invoke(ModuleID receiver, string command, TimeSpan timeout, Action<BaseMessage, byte[]> callback, Action timeoutCallback, byte[] payload = null);
        Task<BaseMessage> Invoke(ModuleID receiver, string command, TimeSpan timeout, byte[] payload = null);

        void Control(ModuleID receiver, string command);
        void Control(ModuleID receiver, string command, TimeSpan timeout, Action<BaseMessage, byte[]> callback, Action timeoutCallback);
        Task<BaseMessage> Control(ModuleID receiver, string command, TimeSpan timeout);

        void SendHook(string hook, BaseMessage msg, byte[] payload = null);
    }
}
