using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Squiggle.Core;
using System.Net;

namespace Squiggle.Bridge.Messages
{
    [ProtoContract]
    class ForwardPresenceMessage : Message
    {
        [ProtoMember(1)]
        public byte[] Message { get; set; }
        [ProtoMember(2)]
        IPAddress IP { get; set; }
        [ProtoMember(3)]
        int Port { get; set; }

        public IPEndPoint BridgeEndPoint
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
