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
        IPEndPoint broadCastEndPoint;
        Guid channelID = Guid.NewGuid();

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public PresenceChannel(int port)
        {
            receiveEndPoint = new IPEndPoint(IPAddress.Any, port);
            broadCastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);

            client = new UdpClient();
            client.EnableBroadcast = true;
            client.DontFragment = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, true);
        }

        public void Start()
        {            
            client.Client.Bind(receiveEndPoint);
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
            client.Send(data, data.Length, broadCastEndPoint);
        }

        void OnReceive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint remoteEndPoint = null;
                var broadcastAddress = (IPEndPoint)ar.AsyncState;
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
            client.BeginReceive(OnReceive, receiveEndPoint);
        }
    }
}
