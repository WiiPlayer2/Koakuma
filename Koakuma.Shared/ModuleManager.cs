using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Koakuma.Shared.Messages;
using System.Timers;
using System.Threading;

using Timer = System.Timers.Timer;
using MessageNetwork;

namespace Koakuma.Shared
{
    class ModuleManager : IKoakuma
    {
        #region Private Fields

        private Dictionary<int, Action<BaseMessage, byte[]>> callbacks;
        private int nextMsgId = 0;
        private Dictionary<int, Timer> timers;

        #endregion Private Fields

        #region Public Constructors

        public ModuleManager(KoakumaNode node, IModule module)
        {
            Module = module;
            Node = node;

            callbacks = new Dictionary<int, Action<BaseMessage, byte[]>>();
            timers = new Dictionary<int, Timer>();

            ModuleID = new ModuleID()
            {
                PublicKey = node.PublicKey,
                ModuleName = module.ID.ToLowerInvariant(),
            };
        }

        #endregion Public Constructors

        #region Protected Properties

        public KoakumaNode Node { get; private set; }
        #endregion Protected Properties

        #region Public Properties
        public ModuleID ModuleID { get; private set; }

        public IModule Module { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Control(ModuleID receiver, string command)
        {
            SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Control,
                Data = command,
            });
        }

        public void Control(ModuleID receiver, string command, TimeSpan timeout, Action<BaseMessage, byte[]> callback, Action timeoutCallback)
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

        public void HandleMessage(KoakumaMessage msg, byte[] payload)
        {
            if(msg.Message.Type == MessageType.Basic)
            {
                switch(msg.Message.Cast<BasicMessage>().Action)
                {
                    case BasicMessage.ActionType.Control:
                        HandleControl(msg.From, msg.Message.Cast<BasicMessage>());
                        return;
                    case BasicMessage.ActionType.Invoke:
                        HandleInvoke(msg.From, msg.ReplyID, msg.Message.Cast<BasicMessage>(), payload);
                        return;
                }
            }
            try
            {
                if (msg.ReplyID.HasValue)
                {
                    if (callbacks.ContainsKey(msg.ReplyID.Value))
                    {
                        callbacks[msg.ReplyID.Value](msg.Message, payload);
                    }
                }
                else
                {
                    Module.OnMessage(msg.From, msg.Message, payload);
                }
            }
            catch { }
        }

        public virtual void HandleControl(ModuleID from, BasicMessage controlMsg) { }

        public virtual void HandleInvoke(ModuleID from, int? replyID, BasicMessage invokeMsg, byte[] payload) { }

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

        public void Invoke(ModuleID receiver, string command, TimeSpan timeout, Action<BaseMessage, byte[]> callback, Action timeoutCallback, byte[] payload = null)
        {
            SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Invoke,
                Data = command,
            }, timeout, callback, timeoutCallback, payload);
        }

        public virtual void SendHook(string hook, BaseMessage msg, byte[] payload = null) { }

        public void SendMessage(ModuleID receiver, BaseMessage msg, byte[] payload = null)
        {
            Node.SendMessage(receiver?.PublicKey, new KoakumaMessage()
            {
                From = ModuleID,
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
                SendMessage(receiver, msg, timeout, (cbmsg, _) =>
                {
                    ret = cbmsg;
                    waitHandle.Set();
                }, () => waitHandle.Set(), payload);
                waitHandle.WaitOne();
                return ret;
            });
        }

        public void SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, Action<BaseMessage, byte[]> callback, Action timeoutCallback, byte[] payload = null)
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

            Node.SendMessage(receiver?.PublicKey, new KoakumaMessage()
            {
                From = ModuleID,
                To = receiver,
                Message = msg,
                ReplyID = id,
            }, payload);
            timer.Start();
        }
        #endregion Public Methods
    }
}
