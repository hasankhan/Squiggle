using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Squiggle.Core.Presence.Transport.Messages;
using ProtoBuf;

namespace Squiggle.Core.Presence.Transport
{
    [ProtoContract]
    [ProtoInclude(50, typeof(GiveUserInfoMessage))]
    [ProtoInclude(51, typeof(KeepAliveMessage))]
    [ProtoInclude(52, typeof(LoginMessage))]
    [ProtoInclude(53, typeof(LogoutMessage))]
    [ProtoInclude(54, typeof(UserUpdateMessage))]
    [ProtoInclude(55, typeof(PresenceMessage))]
    public class Message
    {
        [ProtoMember(1)]
        public Guid ChannelID { get; set; }
        [ProtoMember(2)]
        string ClientID { get; set; }
        [ProtoMember(3)]
        IPAddress PresenceIP { get; set; }
        [ProtoMember(4)]
        int PresencePort { get; set; }

        /// <summary>
        /// Presence endpoint for the sender
        /// </summary>
        public SquiggleEndPoint Sender
        {
            get { return new SquiggleEndPoint(ClientID, PresenceEndPoint); }
            set
            {
                ClientID = value.ClientID;
                PresenceEndPoint = value.Address;
            }
        }

        IPEndPoint PresenceEndPoint
        {
            get { return new IPEndPoint(PresenceIP, PresencePort); }
            set
            {
                PresenceIP = value.Address;
                PresencePort = value.Port;
            }
        }

        public static TMessage FromSender<TMessage>(UserInfo user) where TMessage:Message, new()
        {
            var message = new TMessage()
            {
                ClientID = user.ID,
                PresenceEndPoint = user.PresenceEndPoint,
            };
            return message;
        }

        public bool IsValid
        {
            get
            {
                bool isValid = !String.IsNullOrEmpty(ClientID) && PresenceEndPoint != null;
                return isValid;
            }
        }

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
            Message message = ProtoBuf.Serializer.Deserialize<MessageSurrogate>(stream).GetMessage();
            return message;
        }

        public override string ToString()
        {
            string output = String.Format("Sender: {0}, Message: {1}", PresenceEndPoint, base.ToString());
            return output;
        }
    }
}
