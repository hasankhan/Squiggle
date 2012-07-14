using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Core.Presence.Transport.Messages
{
    /// <summary>
    /// This message is the reply to GiveUserInfo to give your lates user info to whoever asked for it.
    /// </summary>
    [ProtoContract]
    public class UserInfoMessage : PresenceMessage
    {
        [ProtoMember(1)]
        public int State { get; set; }
    }
}
