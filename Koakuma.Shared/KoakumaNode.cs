using Koakuma.Shared.Messages;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using System.Net;

namespace Koakuma.Shared
{
    public class KoakumaNode : MessageNode<KoakumaMessage>
    {
        #region Public Constructors

        public KoakumaNode(AsymmetricCipherKeyPair keyPair) : base(keyPair)
        {
            throw new NotImplementedException();
        }

        public KoakumaNode(AsymmetricCipherKeyPair keyPair, IPAddress localaddr, int port) : base(keyPair, localaddr, port)
        {
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
