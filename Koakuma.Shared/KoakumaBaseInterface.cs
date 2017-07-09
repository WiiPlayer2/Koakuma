using Koakuma.Shared.Messages;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public class KoakumaBaseInterface
    {
        private InterfaceHookManager<NodeEventHandler> nodeJoinedHooks;
        private InterfaceHookManager<NodeEventHandler> nodeLeftHooks;

        public delegate void NodeEventHandler(KoakumaBaseInterface sender, PublicKey publicKey);

        public KoakumaBaseInterface(ModuleID target, IModule module)
            : this(target, module, new TimeSpan(0, 0, 10))
        { }

        public KoakumaBaseInterface(ModuleID target, IModule module, TimeSpan timeout)
        {
            Target = target;
            Module = module;
            Timeout = timeout;

            nodeJoinedHooks = new InterfaceHookManager<NodeEventHandler>(target, module, "node.joined");
            nodeLeftHooks = new InterfaceHookManager<NodeEventHandler>(target, module, "node.left");
        }

        public ModuleID Target { get; private set; }

        public IModule Module { get; private set; }

        public TimeSpan Timeout { get; set; }

        public void HandleMessage(ModuleID from, BaseMessage msg, byte[] payload)
        {
            if (from == Target)
            {
                var split = msg.Cast<BasicMessage>().Data.Split('|');
                switch (split[0])
                {
                    case "node.joined":
                        nodeJoinedHooks.Invoke(this, PublicKey.Parse(split[1]));
                        break;
                    case "node.left":
                        nodeLeftHooks.Invoke(this, PublicKey.Parse(split[1]));
                        break;
                }
            }
        }

        public event NodeEventHandler NodeJoined
        {
            add
            {
                nodeJoinedHooks.Add(value);
            }
            remove
            {
                nodeJoinedHooks.Remove(value);
            }
        }

        public event NodeEventHandler NodeLeft
        {
            add
            {
                nodeLeftHooks.Add(value);
            }
            remove
            {
                nodeLeftHooks.Remove(value);
            }
        }

        public IEnumerable<string> Modules
        {
            get
            {
                return Module.Koakuma.Invoke(Target, "modules.list", Timeout)
                    .Result.Cast<BasicMessage>().Data.Split(',');
            }
        }
    }
}
