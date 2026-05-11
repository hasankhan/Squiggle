using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat.Transport.Messages
{
    class ActivityInviteMessage : Message
    {
        public Guid ActivityId {get; set; }
        public Guid ActivitySessionId { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public ActivityInviteMessage()
        {
            Metadata = new Dictionary<string, string>();
        }
    }
}
