using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Squiggle.Chat.Services.Presence.Transport
{
    [Serializable]
    public class Message
    {
        public Guid ChannelID { get; set; }
        public string ClientID { get; set; }
        public IPEndPoint PresenceEndPoint { get; set; }

        public static TMessage FromUserInfo<TMessage>(UserInfo user) where TMessage:Message, new()
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
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            return stream.ToArray();
        }

        public static Message Deserialize(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var stream = new MemoryStream(data);
            var formatter = new BinaryFormatter();
            var message = (Message)formatter.Deserialize(stream);
            return message;
        }

        public override string ToString()
        {
            string output = String.Format("Sender: {0}, Message: {1}", PresenceEndPoint, base.ToString());
            return output;
        }
    }
}
