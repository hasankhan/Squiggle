using System;
using System.Net;
using System.Text.Json.Serialization;
using Squiggle.Core.Presence.Transport.Messages;

namespace Squiggle.Core.Presence.Transport
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(GiveUserInfoMessage), "GiveUserInfo")]
    [JsonDerivedType(typeof(KeepAliveMessage), "KeepAlive")]
    [JsonDerivedType(typeof(LoginMessage), "Login")]
    [JsonDerivedType(typeof(LogoutMessage), "Logout")]
    [JsonDerivedType(typeof(UserUpdateMessage), "UserUpdate")]
    [JsonDerivedType(typeof(HiMessage), "Hi")]
    [JsonDerivedType(typeof(HelloMessage), "Hello")]
    [JsonDerivedType(typeof(UserInfoMessage), "UserInfo")]
    public abstract class Message
    {
        public Guid ChannelID { get; set; }

        /// <summary>
        /// Presence endpoint for the sender
        /// </summary>
        public SquiggleEndPoint Sender { get; set; } = null!;

        /// <summary>
        /// Presence endpoint for the recipient
        /// </summary>
        public SquiggleEndPoint? Recipient { get; set; }

        public static TMessage FromSender<TMessage>(IUserInfo user) where TMessage:Message, new()
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
