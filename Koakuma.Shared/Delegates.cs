using Koakuma.Shared.Messages;

namespace Koakuma.Shared
{
    public delegate void MessageCallback(ModuleID from, BaseMessage msg, byte[] payload);
}