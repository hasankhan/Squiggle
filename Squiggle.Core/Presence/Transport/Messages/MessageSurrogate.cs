using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.Reflection;

namespace Squiggle.Core.Presence.Transport.Messages
{
    [ProtoContract(SkipConstructor=true)]
    class MessageSurrogate
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

        public MessageSurrogate(Message message)
        {
            string propName = message.GetType().Name;
            PropertyInfo propInfo = typeof(MessageSurrogate).GetProperty(propName, BindingFlags.Instance | BindingFlags.NonPublic);
            propInfo.SetValue(this, message, null);
        }

        public Message GetMessage()
        {
            var properties = typeof(MessageSurrogate).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            var message = (Message)properties.Select(propInfo => propInfo.GetValue(this, null)).Where(x => x != null).FirstOrDefault();
            return message;
        }

    }
}
