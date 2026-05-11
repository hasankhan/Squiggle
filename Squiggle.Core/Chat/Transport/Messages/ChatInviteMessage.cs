using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat.Transport.Messages
{
    class ChatInviteMessage : Message, IMessageHasParticipants
    {
        public List<SquiggleEndPoint> Participants { get; set; }

        public ChatInviteMessage()
        {
            Participants = new List<SquiggleEndPoint>();
        }
    }
}
