using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport
{
    [Serializable]
    public class Message
    {
        public Guid ChannelID { get; set; }
        public IPEndPoint ChatEndPoint { get; set; }

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
            string output = String.Format("Sender: {0}, Message: {1}", ChatEndPoint, base.ToString());
            return output;
        }
    }
}
