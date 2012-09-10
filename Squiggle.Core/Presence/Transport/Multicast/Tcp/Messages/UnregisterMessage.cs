using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Core.Presence.Transport.Multicast.Tcp.Messages
{
    [ProtoContract]
    public class UnregisterMessage: Message
    {
    }
}
