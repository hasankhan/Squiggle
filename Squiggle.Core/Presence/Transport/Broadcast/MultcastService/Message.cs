using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Squiggle.Core.Presence.Transport.Broadcast.MultcastService.Messages;
using System.Net;

namespace Squiggle.Core.Presence.Transport.Broadcast.MultcastService
{
    [ProtoContract]
    [ProtoInclude(50, typeof(MulticastMessage))]
    [ProtoInclude(51, typeof(RegisterMessage))]
    [ProtoInclude(52, typeof(UnregisterMessage))]
    public class Message
    {
        [ProtoMember(1)]
        IPAddress IP { get; set; }
        [ProtoMember(2)]
        int Port { get; set; }

        public IPEndPoint Sender
        {
            get { return new IPEndPoint(IP, Port); }
            set
            {
                IP = value.Address;
                Port = value.Port;
            }
        }
    }
}
