using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.Net;
using Squiggle.Core.Chat.Transport.Messages;
using System.IO;

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

        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, new MessageSurrogate(this));
            return stream.ToArray();
        }

        public static Message Deserialize(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var stream = new MemoryStream(data);
            Message message = ProtoBuf.Serializer.Deserialize<MessageSurrogate>(stream).GetObject();
            return message;
        }
    }
}
