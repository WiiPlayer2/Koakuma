using Koakuma.Shared;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koakuma
{
    class Program
    {
        private static KoakumaNode node;
        private static string dataFolder;

        static void Main(string[] args)
        {
            dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Koakuma");
            Console.WriteLine($"[  APP  ] {dataFolder}");
            Directory.CreateDirectory(dataFolder);
            Directory.CreateDirectory(Path.Combine(dataFolder, "plugins"));
            Directory.CreateDirectory(Path.Combine(dataFolder, "dependecies"));
            Directory.CreateDirectory(Path.Combine(dataFolder, "configs"));
            Directory.CreateDirectory(Path.Combine(dataFolder, "trusted_keys"));
            Directory.CreateDirectory(Path.Combine(dataFolder, "add_keys"));
            var keyPair = Utilities.GenerateOrLoadKeyPair(Path.Combine(dataFolder, "id_rsa"));

            var baseCfg = LoadConfig("koakuma.base");
            var port = baseCfg.Get("port", -1);
            if (port >= 0)
            {
                var bind = baseCfg.Get<string>("bind", null);
                var bindIP = IPAddress.Any;
                if (bind != null)
                {
                    if (!IPAddress.TryParse(bind, out bindIP))
                    {
                        bindIP = IPAddress.Any;
                    }
                }
                node = new KoakumaNode(keyPair, bindIP, port);
            }
            else
            {
                node = new KoakumaNode(keyPair);
            }

            node.TrustedKeys = new TrustedKeyStore(Path.Combine(dataFolder, "trusted_keys"));
            foreach (var f in Directory.GetFiles(Path.Combine(dataFolder, "add_keys")))
            {
                node.TrustedKeys.Add(f);
                File.Delete(f);
            }

            foreach (var f in Directory.EnumerateFiles(Path.Combine(dataFolder, "dependecies")))
            {
                try
                {
                    Assembly.LoadFile(Path.GetFullPath(f));
                }
                finally { }
            }

            foreach (var mod in Directory.EnumerateFiles(Path.Combine(dataFolder, "plugins"))
                .SelectMany(o => LoadModules(o)))
            {
                try
                {
                    mod.Config = LoadConfig(mod.ID);
                    node.AddModule(mod);
                    Console.WriteLine($"[MODLOAD] {mod.ID.ToLowerInvariant()}");
                }
                finally { }
            }

            var host = baseCfg.Get<string>("host", null);
            port = baseCfg.Get("host_port", -1);
            if (!string.IsNullOrEmpty(host) && port >= 0)
            {
                while (!node.Setup(host, port))
                {
                    Thread.Sleep(30 * 1000);

                    Console.WriteLine("[  APP  ] Retrying");
                    baseCfg.Reload();

                    host = baseCfg.Get<string>("host", null);
                    port = baseCfg.Get("host_port", -1);
                }
                Console.WriteLine("[  APP  ] Connected");
            }
            else
            {
                node.Setup();
            }

            foreach(var service in node.Modules.OfType<IService>())
            {
                try
                {
                    service.Start();
                }
                finally { }
            }

            Console.WriteLine($"[  APP  ] Done");
            Thread.Sleep(-1);
        }

        private static ModuleConfig LoadConfig(string module)
        {
            Console.WriteLine($"[CFGLOAD] ./configs/{module.ToLowerInvariant()}.cfg");
            return new ModuleConfig(Path.Combine(dataFolder, "configs", $"{module.ToLowerInvariant()}.cfg"));
        }

        private static IEnumerable<IModule> LoadModules(string assemblyFile)
        {
            try
            {
                var ass = Assembly.LoadFile(Path.GetFullPath(assemblyFile));

                foreach (var type in ass.ExportedTypes
                    .Where(o => o.GetInterfaces()
                    .Contains(typeof(IModule))))
                {
                    try
                    {
                        yield return Activator.CreateInstance(type) as IModule;
                    }
                    finally { }
                }
            }
            finally { }
        }
    }
}
