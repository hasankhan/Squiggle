using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat.Transport.Messages
{
    class SessionInfoMessage : Message, IMessageHasParticipants
    {
        public List<SquiggleEndPoint> Participants { get; set; }

        public SessionInfoMessage()
        {
            Participants = new List<SquiggleEndPoint>();
        }
    }
}
