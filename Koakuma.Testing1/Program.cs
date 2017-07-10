using Koakuma.Shared;
using MessageNetwork;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koakuma.Testing1
{
    class Program
    {
        static void Main(string[] args)
        {
            var keyPair = Utilities.GenerateOrLoadKeyPair("test.key");
            var node = new KoakumaNode(keyPair, IPAddress.Any, 12367);
            node.Setup();
            node.TrustedKeys.Add(Utilities.LoadKeyPair("other.key").Public as RsaKeyParameters);
            Thread.Sleep(-1);
        }
    }
}
