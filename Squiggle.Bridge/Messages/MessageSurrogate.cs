using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Utilities.Serialization;
using ProtoBuf;

namespace Squiggle.Bridge.Messages
{
    [ProtoContract(SkipConstructor=true)]
    class MessageSurrogate: SerializationSurrogate<Message>
    {
        [ProtoMember(1)]
        ForwardChatMessage ForwardChatMessage { get; set; }
        [ProtoMember(2)]
        ForwardPresenceMessage ForwardPresenceMessage { get; set; }

        public MessageSurrogate(Message message) : base(message) { }
    }
}
