using System;
using ProtoBuf;

namespace Squiggle.Core.Presence.Transport.Messages
{
    /// <summary>
    /// Users send you this message to give you their information when you multicast a login message to them.
    /// </summary>
    [ProtoContract]
    public class HiMessage: PresenceMessage
    {
    }
}
