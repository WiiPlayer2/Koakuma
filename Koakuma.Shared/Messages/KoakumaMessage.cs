using MessageNetwork.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Koakuma.Shared.Messages
{
    [Serializable]
    public class KoakumaMessage : CastableMessage<KoakumaMessage>
    {
        public ModuleID From { get; set; }

        public ModuleID To { get; set; }

        public int? ReplyID { get; set; }

        [JsonIgnore]
        public BaseMessage Message { get; set; }

        [JsonProperty("Message")]
        public JObject MessageJson
        {
            get
            {
                if (Message == null)
                {
                    return null;
                }
                return JObject.FromObject(Message);
            }
            set
            {
                if (value == null)
                {
                    Message = null;
                    return;
                }
                Message = value.ToObject<BaseMessage>();
                Message.JObject = value;
            }
        }
    }
}