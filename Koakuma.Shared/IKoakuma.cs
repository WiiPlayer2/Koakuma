using Koakuma.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public interface IKoakuma
    {
        void SendMessage(ModuleID receiver, BaseMessage msg, byte[] payload = null);
        void SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, Action<BaseMessage> callback, Action timeoutCallback, byte[] payload = null);
        Task<BaseMessage> SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, byte[] payload = null);

        void Invoke(ModuleID receiver, string command, byte[] payload = null);
        void Invoke(ModuleID receiver, string command, TimeSpan timeout, Action<BaseMessage> callback, Action timeoutCallback, byte[] payload = null);
        Task<BaseMessage> Invoke(ModuleID receiver, string command, TimeSpan timeout, byte[] payload = null);

        void Control(ModuleID receiver, string command);
    }
}
