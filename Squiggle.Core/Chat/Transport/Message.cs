using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ProtoBuf;
using Squiggle.Core.Chat.Transport.Messages;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Core.Chat.Transport
{
    [ProtoContract]
    [ProtoInclude(50, typeof(ActivityCancelMessage))]
    [ProtoInclude(51, typeof(ActivityDataMessage))]
    [ProtoInclude(52, typeof(ActivityInviteAcceptMessage))]
    [ProtoInclude(53, typeof(ActivityInviteMessage))]
    [ProtoInclude(54, typeof(BuzzMessage))]
    [ProtoInclude(55, typeof(ChatInviteMessage))]
    [ProtoInclude(56, typeof(ChatJoinMessage))]
    [ProtoInclude(57, typeof(ChatLeaveMessage))]
    [ProtoInclude(58, typeof(GiveSessionInfoMessage))]
    [ProtoInclude(59, typeof(SessionInfoMessage))]
    [ProtoInclude(60, typeof(TextMessage))]
    [ProtoInclude(61, typeof(UpdateTextMessage))]
    [ProtoInclude(62, typeof(UserTypingMessage))]
    public abstract class Message
    {
        [ProtoMember(1)]
        public Guid SessionId { get; set; }
        /// <summary>
        /// Chat endpoint for the sender
        /// </summary>
        [ProtoMember(2)]
        public SquiggleEndPoint Sender { get; set; }

        /// <summary>
        /// Chat endpoint for the recipient
        /// </summary>
        [ProtoMember(3)]
        public SquiggleEndPoint Recipient { get; set; }
    }
}
