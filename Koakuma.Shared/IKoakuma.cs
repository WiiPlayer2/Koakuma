using Koakuma.Shared.Messages;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public interface IKoakuma
    {
        ModuleID ModuleID { get; }

        ILogger Logger { get; }

        void SendMessage(ModuleID receiver, BaseMessage msg, byte[] payload = null);

        void SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, MessageCallback callback, Action timeoutCallback, byte[] payload = null);

        Task<BaseMessage> SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, byte[] payload = null);

        void SendRawMessage(ModuleID receiver, JObject msg, byte[] payload = null);

        void SendRawMessage(ModuleID receiver, JObject msg, TimeSpan timeout, MessageCallback callback, Action timeoutCallback, byte[] payload = null);

        Task<BaseMessage> SendRawMessage(ModuleID receiver, JObject msg, TimeSpan timeout, byte[] payload = null);

        void Invoke(ModuleID receiver, string command, byte[] payload = null);

        void Invoke(ModuleID receiver, string command, TimeSpan timeout, MessageCallback callback, Action timeoutCallback, byte[] payload = null);

        Task<BaseMessage> Invoke(ModuleID receiver, string command, TimeSpan timeout, byte[] payload = null);

        void Control(ModuleID receiver, string command);

        void Control(ModuleID receiver, string command, TimeSpan timeout, MessageCallback callback, Action timeoutCallback);

        Task<BaseMessage> Control(ModuleID receiver, string command, TimeSpan timeout);

        void SendHook(string hook, BaseMessage msg, byte[] payload = null);
    }
}