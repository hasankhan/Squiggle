using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Presence.Transport.Messages
{
    /// <summary>
    /// This message is the reply to GiveUserInfo to give your lates user info to whoever asked for it.
    /// </summary>
    [Serializable]
    public class UserInfoMessage : PresenceMessage
    {
        public int State { get; set; }
    }
}
