using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared.Messages
{
    [Serializable]
    public class ModuleID
    {
        public PublicKey PublicKey { get; private set; }

        public string ModuleName { get; set; }
    }
}
