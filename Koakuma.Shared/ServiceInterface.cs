using Koakuma.Shared.Messages;
using MessageNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    public abstract class ServiceInterface
    {
        protected ServiceInterface(ModuleID target, IModule module, TimeSpan timeout)
        {
            Target = target;
            Module = module;
            Timeout = timeout;
        }

        public ModuleID Target { get; private set; }

        public IModule Module { get; private set; }

        public TimeSpan Timeout { get; set; }

        public void HandleMessage(ModuleID from, BaseMessage msg, byte[] payload)
        {
            if (from == Target)
            {
                HandleMessageInternal(from, msg, payload);
            }
        }

        protected abstract void HandleMessageInternal(ModuleID from, BaseMessage msg, byte[] payload);

        protected Task<BaseMessage> Invoke(string command, byte[] payload = null)
        {
            return Module.Koakuma.Invoke(Target, command, Timeout, payload);
        }

        protected static IEnumerable<PublicKey> Find(string name, PublicKey key, IModule module)
        {
            if (key != null)
            {
                var baseInterface = KoakumaBaseInterface.Bind(key, module);
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
                        yield return queue.Dequeue();
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
        }
    }
