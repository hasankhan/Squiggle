using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Core.Chat.Transport.Messages
{
    [ProtoContract]
    class AppInviteMessage : Message
    {
        [ProtoMember(1)]
        public Guid AppId {get; set; }
        [ProtoMember(2)]
        public Guid AppSessionId { get; set; }
        [ProtoMember(3)]
        public Dictionary<string, string> Metadata { get; set; }

        public AppInviteMessage()
        {
            Metadata = new Dictionary<string, string>();
        }
    }
}
