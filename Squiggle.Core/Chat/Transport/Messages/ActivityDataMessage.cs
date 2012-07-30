using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Core.Chat.Transport.Messages
{
    [ProtoContract]
    class ActivityDataMessage : Message
    {
        [ProtoMember(1)]
        public byte[] Data { get; set; }
    }
}
