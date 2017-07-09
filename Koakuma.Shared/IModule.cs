using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public interface IModule
    {
        ModuleFeatures Features { get; }
        string ID { get; }

        IKoakuma Koakuma { get; set; }

        void Load();
        void Unload();
    }
}
