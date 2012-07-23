using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Squiggle.Utilities.Serialization;
using Squiggle.Bridge.Messages;

namespace Squiggle.Bridge
{
    [ProtoContract]
    [ProtoInclude(50, typeof(ForwardPresenceMessage))]
    [ProtoInclude(51, typeof(ForwardChatMessage))]
    public abstract class Message
    {
    }
}
