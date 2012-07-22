using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Bridge.Messages
{
    [ProtoContract]
    [ProtoInclude(50, typeof(ForwardPresenceMessage))]
    [ProtoInclude(51, typeof(ForwardChatMessage))]
    public abstract class Message
    {
        public byte[] Serialize()
        {
            return SerializationHelper.Serialize<MessageSurrogate>(new MessageSurrogate(this));
        }

        public static Message Deserialize(byte[] data)
        {
            var message = SerializationHelper.Deserialize<MessageSurrogate>(data).GetObject();
            return message;
        }
    }
}
