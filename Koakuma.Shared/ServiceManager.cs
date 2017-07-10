using Koakuma.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    class ServiceManager : ModuleManager
    {
        #region Private Fields

        private Dictionary<string, HashSet<ModuleID>> registeredHooks;
        private IService service;

        #endregion Private Fields

        #region Public Constructors

        public ServiceManager(KoakumaNode node, IService service)
            : base(node, service)
        {
            this.service = service;
            registeredHooks = new Dictionary<string, HashSet<ModuleID>>();
        }

        #endregion Public Constructors

        #region Public Methods

        public override void HandleControl(ModuleID from, BasicMessage controlMsg)
        {
            var splits = controlMsg.Data.Split(':');
            switch (splits[0])
            {
                case "Start":
                    service.Start();
                    break;
                case "Stop":
                    service.Stop();
                    break;
                case "RegisterHook":
                    RegisterHook(from, splits[1]);
                    break;
                case "UnregisterHook":
                    UnregisterHook(from, splits[1]);
                    break;
            }
        }

        public override void HandleInvoke(ModuleID from, int? replyID, BasicMessage invokeMsg, byte[] payload)
        {
            var ret = service.Invoke(from, invokeMsg.Data, payload);
            if(replyID.HasValue && ret != null)
            {
                Node.SendMessage(from.PublicKey, new KoakumaMessage()
                {
                    From = ModuleID,
                    To = from,
                    ReplyID = replyID,
                    Message = ret,
                });
            }
        }

        public override void SendHook(string hook, BaseMessage msg, byte[] payload = null)
        {
            if (registeredHooks.ContainsKey(hook))
            {
                foreach (var id in registeredHooks[hook])
                {
                    SendMessage(id, msg, payload);
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void RegisterHook(ModuleID from, string hook)
        {
            if (!registeredHooks.ContainsKey(hook))
            {
                lock (registeredHooks)
                {
                    registeredHooks[hook] = new HashSet<ModuleID>();
                }
            }
            lock (registeredHooks[hook])
            {
                registeredHooks[hook].Add(from);
            }
        }

        private void UnregisterHook(ModuleID from, string hook)
        {
            var isEmpty = false;
            if (registeredHooks.ContainsKey(hook) && registeredHooks[hook].Contains(from))
            {
                lock (registeredHooks[hook])
                {
                    registeredHooks[hook].Remove(from);
                    isEmpty = !registeredHooks[hook].Any();
                }
            }
            if (isEmpty)
            {
                lock (registeredHooks)
                {
                    registeredHooks.Remove(hook);
                }
            }
        }

        #endregion Private Methods
    }
}
