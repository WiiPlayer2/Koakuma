using Koakuma.Shared.Messages;
using MessageNetwork;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Koakuma.Shared
{
    public class KoakumaNode : MessageNode<KoakumaMessage>
    {

        #region Private Fields

        private static Regex moduleRegex = new Regex(@"^\w+(\.\w+)*$");

        private ILogger logger = new NullLogger();

        private Dictionary<string, IModule> modules = new Dictionary<string, IModule>();

        #endregion Private Fields

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

        #region Public Properties

        public ILogger Logger
        {
            get
            {
                return logger;
            }
            set
            {
                if (value == null)
                {
                    logger = new NullLogger();
                }
                else
                {
                    logger = value;
                }
            }
        }
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
            if (modules.ContainsKey(module.ID.ToLowerInvariant()) && module.ID != "koakuma.base")
            {
                lock (modules)
                {
                    modules.Remove(module.ID.ToLowerInvariant());
                }
                module.Unload();
            }
        }

        public override void SendMessage(RsaKeyParameters receiver, KoakumaMessage message, byte[] payload = null)
        {
            if (receiver == null || !PublicKey.Equals(receiver))
            {
                base.SendMessage(receiver, message, payload);
            }

            if (receiver == null || PublicKey.Equals(receiver))
            {
                KoakumaNode_MessageReceived(this, receiver, receiver == null, message, payload);
            }
        }

        #endregion Public Methods

        #region Private Methods

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
                var mods = Modules;
                if (message.From.PublicKey == PublicKey)
                {
                    mods = mods.Where(o => o.ID != message.From.ModuleName);
                }

                foreach (var m in mods)
                {
                    try
                    {
                        (m.Koakuma as ModuleManager).HandleMessage(message, payload);
                    }
                    catch { }
                }
            }
        }

        #endregion Private Methods

        #region Private Classes

        private class Base : IService
        {

            #region Public Constructors

            public Base(KoakumaNode node)
            {
                Node = node;
                Node.NodeJoined += (_, key) => SendNodeHook("node.joined", key);
                Node.NodeLeft += (_, key) => SendNodeHook("node.left", key);
            }

            #endregion Public Constructors

            #region Public Properties

            public ModuleConfig Config { get; set; }
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
            public KoakumaNode Node { get; private set; }

            #endregion Public Properties

            #region Public Methods

            public BaseMessage Invoke(ModuleID from, string command, byte[] payload = null)
            {
                switch (command)
                {
                    case "modules.list":
                        return new BasicMessage()
                        {
                            Action = BasicMessage.ActionType.Data,
                            Data = string.Join(",", Node.Modules.Select(o => o.ID.ToLowerInvariant())),
                        };
                }
                return null;
            }

            public void Load()
            {
            }

            public void OnMessage(ModuleID from, BaseMessage msg, byte[] payload)
            {
            }

            public void Reload()
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

            #endregion Public Methods

            #region Private Methods

            private void SendNodeHook(string hook, PublicKey key)
            {
                Koakuma.SendHook(hook, new BasicMessage()
                {
                    Action = BasicMessage.ActionType.Data,
                    Data = $"{hook}|{key}",
                });
            }

            #endregion Private Methods

        }

        #endregion Private Classes
    }
}