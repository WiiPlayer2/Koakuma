using MessageNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared.Messages
{
    public class BaseMessage : CastableMessage<BaseMessage>
    {
        public virtual MessageType Type { get; protected set; }
    }
}
