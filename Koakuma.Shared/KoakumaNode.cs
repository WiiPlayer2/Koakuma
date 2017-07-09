using Koakuma.Shared.Messages;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using System.Net;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text.RegularExpressions;

namespace Koakuma.Shared
{
    public class KoakumaNode : MessageNode<KoakumaMessage>
    {
        private static Regex moduleRegex = new Regex(@"^\w+(\.\w+)*$");

        private class Base : IService
        {
            public Base(KoakumaNode node)
            {
                Node = node;
                Node.NodeJoined += (_, key) => SendNodeHook("node.joined", key);
                Node.NodeLeft += (_, key) => SendNodeHook("node.left", key);
            }

            public KoakumaNode Node { get; private set; }

            public ModuleFeatures Features { get { return ModuleFeatures.Service; } }

            public IEnumerable<string> Hooks
            {
                get
                {
                    return new[]
                    {
                        "node.joined",
                        "node.left",
                    };
                }
            }

            private void SendNodeHook(string hook, PublicKey key)
            {
                Koakuma.SendHook(hook, new BasicMessage()
                {
                    Action = BasicMessage.ActionType.Data,
                    Data = $"{hook}:{key.ExponentBytes.Length}:{key.ModulusBytes.Length}",
                }, key.ExponentBytes.Concat(key.ModulusBytes).ToArray());
            }

            public string ID { get { return "koakuma.base"; } }

            public IEnumerable<string> Invokes
            {
                get
                {
                    return new[]
                    {
                        "modules.list",
                    };
                }
            }

            public IKoakuma Koakuma { get; set; }

            public ModuleConfig Config { get; set; }

            public void Invoke(ModuleID from, string command, byte[] payload = null)
            {
                switch(command)
                {
                    case "modules.list":
                        Koakuma.SendMessage(from, new BasicMessage()
                        {
                            Action = BasicMessage.ActionType.Data,
                            Data = string.Join(",", Node.Modules.Select(o => o.ID.ToLowerInvariant())),
                        });
                        break;
                }
            }

            public void Load()
            {
            }

            public void Start()
            {
            }

            public void Stop()
            {
            }

            public void Unload()
            {
            }

            public void Reload()
            {
            }

            public void OnMessage(ModuleID from, BaseMessage msg, byte[] payload)
            {
            }
        }

        private Dictionary<string, IModule> modules = new Dictionary<string, IModule>();

        #region Public Constructors
        
        public KoakumaNode(AsymmetricCipherKeyPair keyPair) : base(keyPair)
        {
            Init();
        }

        public KoakumaNode(AsymmetricCipherKeyPair keyPair, IPAddress localaddr, int port) : base(keyPair, localaddr, port)
        {
            Init();
        }

        #endregion Public Constructors

        private void Init()
        {
            MessageReceived += KoakumaNode_MessageReceived;

            AddModule(new Base(this));
        }

        private void KoakumaNode_MessageReceived(MessageNode<KoakumaMessage> sender, RsaKeyParameters senderKey, bool isPublic, KoakumaMessage message, byte[] payload)
        {
            if (message.To.ModuleName != null)
            {
                if (modules.ContainsKey(message.To.ModuleName.ToLowerInvariant()))
                {
                    var mod = modules[message.To.ModuleName.ToLowerInvariant()];
                    try
                    {
                        (mod.Koakuma as ModuleManager).HandleMessage(message, payload);
                    }
                    catch { }
                }
            }
            else
            {
                foreach (var m in Modules)
                {
                    try
                    {
                        (m.Koakuma as ModuleManager).HandleMessage(message, payload);
                    }
                    catch { }
                }
            }
        }
        
        #region Public Properties

        public IEnumerable<IModule> Modules
        {
            get
            {
                lock (modules)
                {
                    return modules.Values.ToList();
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void AddModule(IModule module)
        {
            if (moduleRegex.IsMatch(module.ID) && !modules.ContainsKey(module.ID.ToLowerInvariant()))
            {
                switch (module.Features)
                {
                    case ModuleFeatures.Service:
                        module.Koakuma = new ServiceManager(this, module as IService);
                        break;
                    default:
                        module.Koakuma = new ModuleManager(this, module);
                        break;
                }
                lock (modules)
                {
                    modules[module.ID.ToLowerInvariant()] = module;
                }

                module.Load();
            }
        }

        public void RemoveModule(IModule module)
        {
            if(modules.ContainsKey(module.ID.ToLowerInvariant()))
            {
                lock(modules)
                {
                    modules.Remove(module.ID.ToLowerInvariant());
                }
                module.Unload();
            }
        }

        #endregion Public Methods
    }
}
