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
