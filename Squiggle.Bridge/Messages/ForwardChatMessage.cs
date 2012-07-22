using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Bridge.Messages
{
    [ProtoContract]
    class ForwardChatMessage: Message
    {
        [ProtoMember(1)]
        public byte[] Message { get; set; }
    }
}
