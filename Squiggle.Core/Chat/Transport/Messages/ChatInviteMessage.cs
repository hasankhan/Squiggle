using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Core.Chat.Transport.Messages
{
    [ProtoContract]
    class ChatInviteMessage : Message, IMessageHasParticipants
    {
        [ProtoMember(1)]
        public List<SquiggleEndPoint> Participants { get; set; }

        public ChatInviteMessage()
        {
            Participants = new List<SquiggleEndPoint>();
        }
    }
}
