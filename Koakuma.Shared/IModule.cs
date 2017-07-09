using Koakuma.Shared.Messages;
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

        ModuleConfig Config { get; set; }

        void Load();
        void Reload();
        void Unload();

        void OnMessage(ModuleID from, BaseMessage msg, byte[] payload);
    }
}
