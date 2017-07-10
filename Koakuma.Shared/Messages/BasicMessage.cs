using System;

namespace Koakuma.Shared.Messages
{
    [Serializable]
    public class BasicMessage : BaseMessage
    {
        public enum ActionType
        {
            Control,
            Invoke,
            Data,
        }

        public override MessageType Type
        {
            get { return MessageType.Basic; }

            protected set { }
        }

        public ActionType Action { get; set; }

        public string Data { get; set; }
    }
}