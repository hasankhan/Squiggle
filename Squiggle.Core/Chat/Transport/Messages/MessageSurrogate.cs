using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Squiggle.Utilities;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Core.Chat.Transport.Messages
{
    [ProtoContract(SkipConstructor = true)]
    class MessageSurrogate : SerializationSurrogate<Message>
    {
        [ProtoMember(1)]
        AppCancelMessage AppCancelMessage { get; set; }
        [ProtoMember(2)]
        AppDataMessage AppDataMessage { get; set; }
        [ProtoMember(3)]
        AppInviteAcceptMessage AppInviteAcceptMessage { get; set; }
        [ProtoMember(4)]
        AppInviteMessage AppInviteMessage { get; set; }
        [ProtoMember(5)]
        BuzzMessage BuzzMessage { get; set; }
        [ProtoMember(6)]
        ChatInviteMessage ChatInviteMessage { get; set; }
        [ProtoMember(7)]
        ChatJoinMessage ChatJoinMessage { get; set; }
        [ProtoMember(8)]
        ChatLeaveMessage ChatLeaveMessage { get; set; }
        [ProtoMember(9)]
        GiveSessionInfoMessage GiveSessionInfoMessage { get; set; }
        [ProtoMember(10)]
        SessionInfoMessage SessionInfoMessage { get; set; }
        [ProtoMember(11)]
        TextMessage TextMessage { get; set; }
        [ProtoMember(12)]
        UserTypingMessage UserTypingMessage { get; set; }

        public MessageSurrogate(Message message) : base(message) { }
    }
}
