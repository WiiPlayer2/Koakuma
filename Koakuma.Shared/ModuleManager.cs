using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Koakuma.Shared.Messages;
using System.Timers;
using System.Threading;

using Timer = System.Timers.Timer;

namespace Koakuma.Shared
{
    class ModuleManager : IKoakuma
    {
        #region Private Fields

        private Dictionary<int, Action<BaseMessage>> callbacks;
        private IModule mod;
        private int nextMsgId = 0;
        private Dictionary<int, Timer> timers;

        #endregion Private Fields
        
        #region Public Constructors

        public ModuleManager(KoakumaNode node, IModule module)
        {
            mod = module;
            Node = node;

            callbacks = new Dictionary<int, Action<BaseMessage>>();
            timers = new Dictionary<int, Timer>();

            ID = new ModuleID()
            {
                PublicKey = node.PublicKey,
                ModuleName = module.ID,
            };
        }

        #endregion Public Constructors

        #region Protected Properties

        protected ModuleID ID { get; private set; }
        protected KoakumaNode Node { get; private set; }

        #endregion Protected Properties

        #region Public Methods

        public void Control(ModuleID receiver, string command)
        {
            SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Control,
                Data = command,
            });
        }

        public void Control(ModuleID receiver, string command, TimeSpan timeout, Action<BaseMessage> callback, Action timeoutCallback)
        {
            SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Control,
                Data = command,
            }, timeout, callback, timeoutCallback);
        }

        public Task<BaseMessage> Control(ModuleID receiver, string command, TimeSpan timeout)
        {
            return SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Control,
                Data = command,
            }, timeout);
        }

        public void Invoke(ModuleID receiver, string command, byte[] payload = null)
        {
            SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Invoke,
                Data = command,
            }, payload);
        }

        public Task<BaseMessage> Invoke(ModuleID receiver, string command, TimeSpan timeout, byte[] payload = null)
        {
            return SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Invoke,
                Data = command,
            }, timeout, payload);
        }

        public void Invoke(ModuleID receiver, string command, TimeSpan timeout, Action<BaseMessage> callback, Action timeoutCallback, byte[] payload = null)
        {
            SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Invoke,
                Data = command,
            }, timeout, callback, timeoutCallback, payload);
        }

        public void SendHook(string hook, BaseMessage msg, byte[] payload = null)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(ModuleID receiver, BaseMessage msg, byte[] payload = null)
        {
            Node.SendMessage(receiver?.PublicKey, new KoakumaMessage()
            {
                From = ID,
                To = receiver,
                Message = msg,
            }, payload);
        }

        public Task<BaseMessage> SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, byte[] payload = null)
        {
            return Task.Run(() =>
            {
                var waitHandle = new ManualResetEvent(false);
                BaseMessage ret = null;
                SendMessage(receiver, msg, timeout, cbmsg =>
                {
                    ret = cbmsg;
                    waitHandle.Set();
                }, () => waitHandle.Set(), payload);
                return ret;
            });
        }

        public void SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, Action<BaseMessage> callback, Action timeoutCallback, byte[] payload = null)
        {
            var id = 0;
            lock (callbacks)
            {
                id = nextMsgId++;
            }
            callbacks[id] = callback;

            var timer = new Timer(timeout.TotalMilliseconds)
            {
                AutoReset = false,
            };
            timers[id] = timer;
            timer.Elapsed += (_, __) =>
            {
                callbacks.Remove(id);
                timers.Remove(id);
                timer.Stop();

                timeoutCallback?.Invoke();
            };

            SendMessage(receiver, msg, payload);
            timer.Start();
        }

        #endregion Public Methods
    }
}
