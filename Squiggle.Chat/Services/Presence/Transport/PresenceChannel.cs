using System;
using System.Net.Sockets;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport
{
    class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint Sender { get; set; }
        public Message Message { get; set; }
    }

    class PresenceChannel
    {
        UdpClient client;
        IPEndPoint receiveEndPoint;
        IPEndPoint multicastEndPoint;
        Guid channelID = Guid.NewGuid();

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public PresenceChannel(int port)
        {
            receiveEndPoint = new IPEndPoint(IPAddress.Any, port);
            multicastEndPoint = new IPEndPoint(IPAddress.Parse("224.10.11.12"), port);

            client = new UdpClient();
            client.DontFragment = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void Start()
        {            
            client.Client.Bind(receiveEndPoint);
            client.JoinMulticastGroup(multicastEndPoint.Address);
            BeginReceive();
        }        

        public void Stop()
        {
            client.Close();
        }

        public void SendMessage(Message message)
        {
            message.ChannelID = channelID;
            byte[] data = message.Serialize();
            client.Send(data, data.Length, multicastEndPoint);
        }

        void OnReceive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint remoteEndPoint = null;
                byte[] data = client.EndReceive(ar, ref remoteEndPoint);
                var message = Message.Deserialize(data);
                BeginReceive();
                if (!message.ChannelID.Equals(channelID))
                {
                    var args = new MessageReceivedEventArgs()
                    {
                        Message = message,
                        Sender = remoteEndPoint
                    };
                    MessageReceived(this, args);
                }
            }
            catch (Exception) { }
        }

        void BeginReceive()
        {
            client.BeginReceive(OnReceive, null);
        }
    }
}
