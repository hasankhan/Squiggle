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
    [ProtoInclude(50, typeof(AppCancelMessage))]
    [ProtoInclude(51, typeof(AppDataMessage))]
    [ProtoInclude(52, typeof(AppInviteAcceptMessage))]
    [ProtoInclude(53, typeof(AppInviteMessage))]
    [ProtoInclude(54, typeof(BuzzMessage))]
    [ProtoInclude(55, typeof(ChatInviteMessage))]
    [ProtoInclude(56, typeof(ChatJoinMessage))]
    [ProtoInclude(57, typeof(ChatLeaveMessage))]
    [ProtoInclude(58, typeof(GiveSessionInfoMessage))]
    [ProtoInclude(59, typeof(SessionInfoMessage))]
    [ProtoInclude(60, typeof(TextMessage))]
    [ProtoInclude(61, typeof(UserTypingMessage))]
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

        public byte[] Serialize()
        {
            return SerializationHelper.Serialize<MessageSurrogate>(new MessageSurrogate(this));
        }

        public static Message Deserialize(byte[] data)
        {
            Message message = SerializationHelper.Deserialize<MessageSurrogate>(data).GetObject();
            return message;
        }
    }
}
