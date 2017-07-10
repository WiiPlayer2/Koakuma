using MessageNetwork.Messages;
using System;

namespace Koakuma.Shared.Messages
{
    [Serializable]
    public class BaseMessage : CastableMessage<BaseMessage>
    {
        public virtual MessageType Type { get; protected set; }
    }
}