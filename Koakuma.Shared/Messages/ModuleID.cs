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
        public PublicKey PublicKey { get; set; }

        public string ModuleName { get; set; }

        public static bool operator ==(ModuleID id1, ModuleID id2)
        {
            return id1.ModuleName == id2.ModuleName && id1.PublicKey == id2.PublicKey;
        }

        public static bool operator !=(ModuleID id1, ModuleID id2)
        {
            return id1.ModuleName != id2.ModuleName || id1.PublicKey != id2.PublicKey;
        }

        public override bool Equals(object obj)
        {
            if(obj is ModuleID)
            {
                return this == (ModuleID)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return PublicKey.GetHashCode() * 0xFFFF + ModuleName.GetHashCode();
        }
    }
}
