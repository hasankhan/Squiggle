using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Core.Chat.Transport.Messages
{
    [ProtoContract]
    class ActivityInviteMessage : Message
    {
        [ProtoMember(1)]
        public Guid ActivityId {get; set; }
        [ProtoMember(2)]
        public Guid ActivitySessionId { get; set; }
        [ProtoMember(3)]
        public Dictionary<string, string> Metadata { get; set; }

        public ActivityInviteMessage()
        {
            Metadata = new Dictionary<string, string>();
        }
    }
}
