using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;
using Squiggle.Core.Presence.Transport.Messages;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Core.Presence.Transport
{
    [ProtoContract]
    [ProtoInclude(50, typeof(GiveUserInfoMessage))]
    [ProtoInclude(51, typeof(KeepAliveMessage))]
    [ProtoInclude(52, typeof(LoginMessage))]
    [ProtoInclude(53, typeof(LogoutMessage))]
    [ProtoInclude(54, typeof(UserUpdateMessage))]
    [ProtoInclude(55, typeof(PresenceMessage))]
    public abstract class Message
    {
        [ProtoMember(1)]
        public Guid ChannelID { get; set; }

        /// <summary>
        /// Presence endpoint for the sender
        /// </summary>
        [ProtoMember(2)]
        public SquiggleEndPoint Sender { get; set; }

        /// <summary>
        /// Presence endpoint for the recipient
        /// </summary>
        [ProtoMember(3)]
        public SquiggleEndPoint Recipient { get; set; }

        public static TMessage FromSender<TMessage>(UserInfo user) where TMessage:Message, new()
        {
            var message = new TMessage() { Sender = new SquiggleEndPoint(user.ID, user.PresenceEndPoint) };
            return message;
        }

        public override string ToString()
        {
            string output = String.Format("Sender: {0}, Message: {1}", Sender, base.ToString());
            return output;
        }
    }
}
