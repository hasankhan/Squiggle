using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.Net;

namespace Squiggle.Core.Presence.Transport.Multicast.Tcp.Messages
{
    [ProtoContract]
    public class MulticastMessage: Message
    {
        [ProtoMember(3)]
        public byte[] Data { get; set; }
    }
}
