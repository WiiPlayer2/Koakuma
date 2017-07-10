using Koakuma.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public delegate void MessageCallback(ModuleID from, BaseMessage msg, byte[] payload);
}
