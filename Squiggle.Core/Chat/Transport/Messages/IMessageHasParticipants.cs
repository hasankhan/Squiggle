using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat.Transport.Messages
{
    public interface IMessageHasParticipants
    {
        List<SquiggleEndPoint> Participants { get; set; }
    }
}
