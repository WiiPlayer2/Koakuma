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

namespace Koakuma.Shared
{
    public class KoakumaNode : MessageNode<KoakumaMessage>
    {
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

            public void Invoke(string command, BaseMessage msg, byte[] payload = null)
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
        }

        #region Public Constructors

        public KoakumaNode(AsymmetricCipherKeyPair keyPair) : base(keyPair)
        {
            PublicKey = keyPair.Public as RsaKeyParameters;
            throw new NotImplementedException();
        }

        public KoakumaNode(AsymmetricCipherKeyPair keyPair, IPAddress localaddr, int port) : base(keyPair, localaddr, port)
        {
            PublicKey = keyPair.Public as RsaKeyParameters;
            throw new NotImplementedException();
        }

        #endregion Public Constructors

        #region Public Properties

        public IEnumerable<IModule> Modules
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PublicKey PublicKey { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void AddModule(IModule module)
        {
            throw new NotImplementedException();
        }

        public void RemoveModule(IModule module)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}
