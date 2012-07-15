using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.Reflection;
using Squiggle.Utilities;

namespace Squiggle.Core.Presence.Transport.Messages
{
    [ProtoContract(SkipConstructor=true)]
    class MessageSurrogate: SerializationSurrogate<Message>
    {
        [ProtoMember(1)]
        GiveUserInfoMessage GiveUserInfoMessage { get; set; }
        [ProtoMember(2)]
        HelloMessage HelloMessage { get; set; }
        [ProtoMember(3)]
        HiMessage HiMessage { get; set; }
        [ProtoMember(4)]
        KeepAliveMessage KeepAliveMessage { get; set; }
        [ProtoMember(5)]
        LoginMessage LoginMessage { get; set; }
        [ProtoMember(6)]
        LogoutMessage LogoutMessage { get; set; }
        [ProtoMember(7)]
        UserInfoMessage UserInfoMessage { get; set; }
        [ProtoMember(8)]
        UserUpdateMessage UserUpdateMessage { get; set; }

        public MessageSurrogate(Message message): base(message) { }
    }
}
