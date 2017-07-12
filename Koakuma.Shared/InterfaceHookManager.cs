using Koakuma.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Koakuma.Shared
{
    public class InterfaceHookManager<T>
        where T : class
    {
        private HashSet<T> delegates = new HashSet<T>();

        static InterfaceHookManager()
        {
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
            {
                throw new NotSupportedException($"{typeof(T)} is not a delegate.");
            }
        }

        internal InterfaceHookManager(ModuleID target, IModule module, string hook)
        {
            Module = module;
            Hook = hook;
            Target = target;
        }

        public IModule Module { get; private set; }

        public string Hook { get; private set; }

        public ModuleID Target { get; private set; }

        public void Add(T val)
        {
            var wasEmpty = !delegates.Any();
            delegates.Add(val);
            if (wasEmpty)
            {
                Module.Koakuma.Control(Target, $"RegisterHook:{Hook}");
            }
        }

        public void Remove(T val)
        {
            delegates.Remove(val);
            if (!delegates.Any())
            {
                Module.Koakuma.Control(Target, $"UnregisterHook:{Hook}");
            }
        }

        public void Invoke(params object[] args)
        {
            foreach (var d in delegates)
            {
                (d as Delegate).DynamicInvoke(args);
            }
        }
    }
}