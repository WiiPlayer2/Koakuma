using Koakuma.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using Newtonsoft.Json.Linq;

namespace Koakuma.Shared
{
    internal class ModuleManager : IKoakuma
    {
        #region Private Fields

        private Dictionary<int, MessageCallback> callbacks;
        private int nextMsgId = 0;
        private Dictionary<int, Timer> timers;

        #endregion Private Fields

        #region Public Constructors

        public ModuleManager(KoakumaNode node, IModule module)
        {
            Module = module;
            Node = node;

            callbacks = new Dictionary<int, MessageCallback>();
            timers = new Dictionary<int, Timer>();

            ModuleID = new ModuleID()
            {
                PublicKey = node.PublicKey,
                ModuleName = module.ID.ToLowerInvariant(),
            };
            Logger = new ModuleLogger(node, module);
        }

        #endregion Public Constructors

        #region Protected Properties

        public KoakumaNode Node { get; private set; }

        #endregion Protected Properties

        #region Public Properties

        public ModuleID ModuleID { get; private set; }

        public IModule Module { get; private set; }

        public ILogger Logger { get; private set; }

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

        public void Control(ModuleID receiver, string command, TimeSpan timeout, MessageCallback callback, Action timeoutCallback)
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
            if (msg.Message.Type == MessageType.Basic)
            {
                switch (msg.Message.Cast<BasicMessage>().Action)
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
                        callbacks[msg.ReplyID.Value](msg.From, msg.Message, payload);
                    }
                }
                else
                {
                    Module.OnMessage(msg.From, msg.Message, payload);
                }
            }
            catch { }
        }

        public virtual void HandleControl(ModuleID from, BasicMessage controlMsg)
        {
        }

        public virtual void HandleInvoke(ModuleID from, int? replyID, BasicMessage invokeMsg, byte[] payload)
        {
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

        public void Invoke(ModuleID receiver, string command, TimeSpan timeout, MessageCallback callback, Action timeoutCallback, byte[] payload = null)
        {
            SendMessage(receiver, new BasicMessage()
            {
                Action = BasicMessage.ActionType.Invoke,
                Data = command,
            }, timeout, callback, timeoutCallback, payload);
        }

        public virtual void SendHook(string hook, BaseMessage msg, byte[] payload = null)
        {
        }

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
            return SendMessageInternal(receiver, msg, timeout, SendMessage, payload);
        }

        public void SendMessage(ModuleID receiver, BaseMessage msg, TimeSpan timeout, MessageCallback callback, Action timeoutCallback, byte[] payload = null)
        {
            SendMessageInternal(receiver, replyId => new KoakumaMessage()
            {
                From = ModuleID,
                To = receiver,
                Message = msg,
                ReplyID = replyId,
            }, timeout, callback, timeoutCallback, payload);
        }

        public void SendRawMessage(ModuleID receiver, JObject msg, byte[] payload = null)
        {
            Node.SendMessage(receiver?.PublicKey, new KoakumaMessage()
            {
                From = ModuleID,
                To = receiver,
                MessageJson = msg,
            }, payload);
        }

        public Task<BaseMessage> SendRawMessage(ModuleID receiver, JObject msg, TimeSpan timeout, byte[] payload = null)
        {
            return SendMessageInternal(receiver, msg, timeout, SendRawMessage, payload);
        }

        public void SendRawMessage(ModuleID receiver, JObject msg, TimeSpan timeout, MessageCallback callback, Action timeoutCallback, byte[] payload = null)
        {
            SendMessageInternal(receiver, replyId => new KoakumaMessage()
            {
                From = ModuleID,
                To = receiver,
                MessageJson = msg,
                ReplyID = replyId,
            }, timeout, callback, timeoutCallback, payload);
        }

        #endregion Public Methods

        private void SendMessageInternal(ModuleID receiver, Func<int, KoakumaMessage> msgFunc, TimeSpan timeout, MessageCallback callback, Action timeoutCallback, byte[] payload)
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

            Node.SendMessage(receiver?.PublicKey, msgFunc(id), payload);
            timer.Start();
        }

        private Task<BaseMessage> SendMessageInternal<T>(ModuleID receiver, T msg, TimeSpan timeout, Action<ModuleID, T, TimeSpan, MessageCallback, Action, byte[]> sendFunc, byte[] payload)
        {
            return Task.Run(() =>
            {
                var waitHandle = new ManualResetEvent(false);
                BaseMessage ret = null;
                sendFunc(receiver, msg, timeout, (_, cbmsg, __) =>
                {
                    ret = cbmsg;
                    waitHandle.Set();
                }, () => waitHandle.Set(), payload);
                waitHandle.WaitOne();
                return ret;
            });
        }
    }
}