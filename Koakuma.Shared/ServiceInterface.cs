using Koakuma.Shared.Messages;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public abstract class ServiceInterface
    {
        #region Private Fields

        private static Dictionary<Type, Func<PublicKey, IModule, object>> binds;

        private static Dictionary<Type, string> defaultIDs;

        #endregion Private Fields

        #region Protected Constructors

        protected ServiceInterface(ModuleID target, IModule module, TimeSpan timeout)
        {
            Target = target;
            Module = module;
            Timeout = timeout;
        }

        #endregion Protected Constructors

        #region Public Properties

        public IModule Module { get; private set; }

        public ModuleID Target { get; private set; }

        public TimeSpan Timeout { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void HandleMessage(ModuleID from, BaseMessage msg, byte[] payload)
        {
            if (from == Target)
            {
                HandleMessageInternal(from, msg, payload);
            }
        }

        #endregion Public Methods

        private static void CheckRegister<T>()
            where T : ServiceInterface
        {
            if (!defaultIDs.ContainsKey(typeof(T)))
            {
                var attr = typeof(T).GetCustomAttributes(typeof(Defaults), false).FirstOrDefault() as Defaults;
                if (attr == null)
                {
                    throw new InvalidOperationException($"{typeof(T)} is missing an {typeof(Defaults)} attribute.");
                }

                var constructor = typeof(T).GetConstructor(new[] { typeof(ModuleID), typeof(IModule) });
                if (constructor != null)
                {
                    defaultIDs[typeof(T)] = attr.ID;
                    binds[typeof(T)] = (k, m) =>
                    {
                        var ret = constructor.Invoke(new object[] {new ModuleID()
                        {
                            ModuleName = attr.ID,
                            PublicKey = k,
                        }, m}) as ServiceInterface;
                        ret.Timeout = attr.Timeout;
                        return ret;
                    };
                }
                else
                {
                    constructor = typeof(T).GetConstructor(new[] { typeof(ModuleID), typeof(IModule), typeof(TimeSpan) });
                    if (constructor != null)
                    {
                        defaultIDs[typeof(T)] = attr.ID;
                        binds[typeof(T)] = (k, m) => constructor.Invoke(new object[] { new ModuleID()
                        {
                            ModuleName = attr.ID,
                            PublicKey = k,
                        }, m, attr.Timeout});
                    }
                    else
                    {
                        throw new InvalidOperationException($"{typeof(T)} has now suitable constructor.");
                    }
                }
            }
        }

        public static IEnumerable<PublicKey> Find<T>(PublicKey key, IModule module)
            where T : ServiceInterface
        {
            CheckRegister<T>();
            return Find(defaultIDs[typeof(T)], key, module);
        }

        public static T Bind<T>(PublicKey key, IModule module)
            where T : ServiceInterface
        {
            return BindAll<T>(key, module).FirstOrDefault();
        }

        public static IEnumerable<T> BindAll<T>(IModule module)
            where T : ServiceInterface
        {
            return BindAll<T>(null, module);
        }

        public static IEnumerable<T> BindAll<T>(PublicKey key, IModule module)
            where T : ServiceInterface
        {
            CheckRegister<T>();
            return BindAll(defaultIDs[typeof(T)], key, module, (k, m) => binds[typeof(T)](k, m) as T);
        }

        #region Protected Methods

        protected static IEnumerable<T> BindAll<T>(string name, PublicKey key, IModule module, Func<PublicKey, IModule, T> bindFunc)
            where T : ServiceInterface
        {
            return Find(name, key, module)
                .Select(o => bindFunc(o, module));
        }

        protected static IEnumerable<PublicKey> Find(string name, PublicKey key, IModule module)
        {
            if (key != null)
            {
                var baseInterface = new KoakumaBaseInterface(new ModuleID()
                {
                    ModuleName = "koakuma.base",
                    PublicKey = key,
                }, module);
                IEnumerable<string> mods = null;
                try
                {
                    mods = baseInterface.Modules;
                }
                catch { }
                if (mods != null && mods.Contains(name))
                {
                    yield return key;
                }
            }
            else
            {
                var waitHandle = new AutoResetEvent(false);
                var queue = new Queue<PublicKey>();
                var timedOut = false;

                module.Koakuma.Invoke(new ModuleID()
                {
                    ModuleName = "koakuma.base",
                    PublicKey = null,
                }, "modules.list", TimeSpan.FromSeconds(10), (from, msg, _) =>
                {
                    var splits = msg.Cast<BasicMessage>().Data.Split(',');
                    if (splits.Contains(name))
                    {
                        lock (queue)
                        {
                            queue.Enqueue(from.PublicKey);
                        }
                        waitHandle.Set();
                    }
                }, () =>
                {
                    timedOut = true;
                    waitHandle.Set();
                });

                while (!timedOut)
                {
                    waitHandle.WaitOne();
                    lock (queue)
                    {
                        if (queue.Any())
                        {
                            yield return queue.Dequeue();
                        }
                    }
                }

                lock (queue)
                {
                    while (queue.Any())
                    {
                        yield return queue.Dequeue();
                    }
                }
            }
        }

        protected static void Register<T>(string id, Func<PublicKey, IModule, T> bindFunc)
            where T : ServiceInterface
        {
            defaultIDs[typeof(T)] = id;
            binds[typeof(T)] = (k, m) => bindFunc(k, m);
        }

        protected abstract void HandleMessageInternal(ModuleID from, BaseMessage msg, byte[] payload);

        protected Task<BaseMessage> Invoke(string command, byte[] payload = null)
        {
            return Module.Koakuma.Invoke(Target, command, Timeout, payload);
        }

        #endregion Protected Methods

        #region Public Classes

        public sealed class Defaults : Attribute
        {
            #region Public Constructors

            public Defaults(string id)
                : this(id, TimeSpan.FromSeconds(5))
            {

            }

            public Defaults(string id, TimeSpan timeout)
            {
                ID = id;
                Timeout = timeout;
            }

            #endregion Public Constructors

            #region Public Properties

            public string ID { get; set; }

            public TimeSpan Timeout { get; set; }

            #endregion Public Properties
        }

        #endregion Public Classes
    }
}