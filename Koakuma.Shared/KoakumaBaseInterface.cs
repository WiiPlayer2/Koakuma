using Koakuma.Shared.Messages;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Koakuma.Shared
{
    public class KoakumaBaseInterface : ServiceInterface
    {
        private InterfaceHookManager<NodeEventHandler> nodeJoinedHooks;
        private InterfaceHookManager<NodeEventHandler> nodeLeftHooks;

        public delegate void NodeEventHandler(KoakumaBaseInterface sender, PublicKey publicKey);

        static KoakumaBaseInterface()
        {
            Register("koakuma.base", (k, m) => new KoakumaBaseInterface(new ModuleID()
            {
                ModuleName = "koakuma.base",
                PublicKey = k,
            }, m));
        }

        public KoakumaBaseInterface(ModuleID target, IModule module)
            : this(target, module, new TimeSpan(0, 0, 10))
        { }

        public KoakumaBaseInterface(ModuleID target, IModule module, TimeSpan timeout)
            : base(target, module, timeout)
        {
            nodeJoinedHooks = CreateHookManager<NodeEventHandler>("node.joined");
            nodeLeftHooks = CreateHookManager<NodeEventHandler>("node.left");
        }

        protected override void HandleMessageInternal(ModuleID from, BaseMessage msg, byte[] payload)
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
                return Invoke("modules.list").Result.Cast<BasicMessage>().Data.Split(',');
            }
        }
    }
}