using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Squiggle.Core.Presence.Transport.Messages
{
    /// <summary>
    /// This is a reply to Hi message. To give other users your info when they welcome you to network.
    /// </summary>
    [ProtoContract]
    public class HelloMessage: PresenceMessage
    {
    }
}
