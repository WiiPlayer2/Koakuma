using Koakuma.Shared;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Koakuma.Shared.Messages;

namespace Koakuma.Testing2
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            var key = Utilities.GenerateOrLoadKeyPair("test.key");
            var node = new KoakumaNode(key);

            node.AddModule(new Mod());
            Console.WriteLine(node.Setup("localhost", 12367));

            Thread.Sleep(-1);
        }

        private class Mod : IModule
        {
            public ModuleConfig Config { get; set; }

            public ModuleFeatures Features { get { return ModuleFeatures.Default; } }

            public string ID { get { return "testing.1"; } }

            public IKoakuma Koakuma { get; set; }

            private KoakumaBaseInterface baseInterface;

            public void Load()
            {
                baseInterface = new KoakumaBaseInterface(new ModuleID()
                {
                    PublicKey = Koakuma.ModuleID.PublicKey,
                    ModuleName = "koakuma.base",
                }, this);
                baseInterface.NodeJoined += BaseInterface_NodeJoined;
            }

            private void BaseInterface_NodeJoined(KoakumaBaseInterface sender, PublicKey publicKey)
            {
                foreach(var baseInterface in KoakumaBaseInterface.BindAll(this))
                {
                    Console.WriteLine(string.Join(", ", baseInterface.Modules));
                }
            }

            public void OnMessage(ModuleID from, BaseMessage msg, byte[] payload)
            {
                baseInterface.HandleMessage(from, msg, payload);
            }

            public void Reload()
            {
            }

            public void Unload()
            {

            }
        }
    }
}
