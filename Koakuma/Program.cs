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
        private static Dictionary<string, Assembly> deps = new Dictionary<string, Assembly>();
        private static ILogger logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Koakuma");

            var baseCfg = LoadConfig("koakuma.base");
            var level = LogLevel.Verbose;
            Enum.TryParse(baseCfg.Get("log_level", LogLevel.Info.ToString()), out level);
            logger.OutputLevel = level;

            logger.Log(LogLevel.Debug, "APP", dataFolder);


            Directory.CreateDirectory(dataFolder);
            Directory.CreateDirectory(Path.Combine(dataFolder, "plugins"));
            Directory.CreateDirectory(Path.Combine(dataFolder, "dependencies"));
            Directory.CreateDirectory(Path.Combine(dataFolder, "configs"));
            Directory.CreateDirectory(Path.Combine(dataFolder, "trusted_keys"));
            Directory.CreateDirectory(Path.Combine(dataFolder, "add_keys"));
            var keyPair = Utilities.GenerateOrLoadKeyPair(Path.Combine(dataFolder, "id_rsa"));
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

            node.Logger = logger;

            node.TrustedKeys = new TrustedKeyStore(Path.Combine(dataFolder, "trusted_keys"));
            foreach (var f in Directory.GetFiles(Path.Combine(dataFolder, "add_keys")))
            {
                node.TrustedKeys.Add(f);
                File.Delete(f);
            }

            foreach (var f in Directory.EnumerateFiles(Path.Combine(dataFolder, "dependencies")))
            {
                try
                {
                    var ass = Assembly.LoadFile(Path.GetFullPath(f));
                    deps[ass.ToString()] = ass;
                    logger.Log(LogLevel.Debug, "DEPLOAD", ass);
                }
                finally { }
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var mod in Directory.EnumerateFiles(Path.Combine(dataFolder, "plugins"))
                .SelectMany(o => LoadModules(o)))
            {
                try
                {
                    mod.Config = LoadConfig(mod.ID);
                    node.AddModule(mod);
                    logger.Log(LogLevel.Debug, "MODADD", mod.ID.ToLowerInvariant());
                }
                finally { }
            }

            foreach (var mod in node.Modules)
            {
                try
                {
                    mod.Load();
                    logger.Log(LogLevel.Debug, "MODLOAD", mod.ID.ToLowerInvariant());
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

                    logger.Log(LogLevel.Info, "APP", "Retrying");
                    baseCfg.Reload();

                    host = baseCfg.Get<string>("host", null);
                    port = baseCfg.Get("host_port", -1);
                }
                logger.Log(LogLevel.Info, "APP", "Connected");
            }
            else
            {
                node.Setup();
            }

            foreach (var service in node.Modules.OfType<IService>())
            {
                try
                {
                    service.Start();
                }
                finally { }
            }

            logger.Log(LogLevel.Info, "APP", "Done");
            Thread.Sleep(-1);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (deps.ContainsKey(args.Name))
            {
                logger.Log(LogLevel.Debug, "RESOLVE", args.Name);
                return deps[args.Name];
            }
            logger.Log(LogLevel.Debug, "RESOLVE", $"FAILED: {args.Name}");
            return null;
        }

        private static ModuleConfig LoadConfig(string module)
        {
            logger.Log(LogLevel.Debug, "CFGLOAD", $"./configs/{module.ToLowerInvariant()}.cfg");
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
