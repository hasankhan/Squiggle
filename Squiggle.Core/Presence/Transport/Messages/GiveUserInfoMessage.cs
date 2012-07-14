using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Presence.Transport.Messages
{
    /// <summary>
    /// This message is sent by users when they need your updated user info. They can pass in some state that you have to return them with reply (UserInfoMessage).
    /// </summary>
    [Serializable]
    public class GiveUserInfoMessage: Message
    {
        public int State { get; set; }
    }
}
