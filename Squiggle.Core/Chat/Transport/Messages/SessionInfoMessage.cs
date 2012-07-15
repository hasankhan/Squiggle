using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Core.Chat.Transport.Messages
{
    [ProtoContract]
    class SessionInfoMessage: Message
    {
        [ProtoMember(1)]
        public SquiggleEndPoint[] Participants { get; set; }
    }
}
