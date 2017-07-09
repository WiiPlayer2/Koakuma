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
            }

            public KoakumaNode Node { get; private set; }

            public ModuleFeatures Features { get { return ModuleFeatures.Service; } }

            public IEnumerable<string> Hooks
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string ID { get { return "koakuma.base"; } }

            public IEnumerable<string> Invokes
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IKoakuma Koakuma { get; set; }

            public ModuleConfig Config { get; set; }

            public void Invoke(ModuleID from ,string command, byte[] payload = null)
            {
                throw new NotImplementedException();
            }

            public void Load()
            {
                throw new NotImplementedException();
            }

            public void Start()
            {
                throw new NotImplementedException();
            }

            public void Stop()
            {
                throw new NotImplementedException();
            }

            public void Unload()
            {
                throw new NotImplementedException();
            }

            public void Reload()
            {
                throw new NotImplementedException();
            }

            public void OnMessage(BaseMessage msg, byte[] payload)
            {
                throw new NotImplementedException();
            }
        }

        private Dictionary<string, IModule> modules = new Dictionary<string, IModule>();

        #region Public Constructors

        //TODO: export publickey from MessageNode
        public KoakumaNode(AsymmetricCipherKeyPair keyPair) : base(keyPair)
        {
            PublicKey = keyPair.Public as RsaKeyParameters;
            Init();
        }

        public KoakumaNode(AsymmetricCipherKeyPair keyPair, IPAddress localaddr, int port) : base(keyPair, localaddr, port)
        {
            PublicKey = keyPair.Public as RsaKeyParameters;
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

        public PublicKey PublicKey { get; private set; }

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
