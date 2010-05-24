using System;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

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
        bool started;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public PresenceChannel(IPEndPoint multicastEndPoint)
        {
            receiveEndPoint = new IPEndPoint(IPAddress.Any, multicastEndPoint.Port);
            this.multicastEndPoint = multicastEndPoint;            
        }

        public void Start()
        {
            started = true;
            client = new UdpClient();
            client.DontFragment = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(receiveEndPoint);
            client.JoinMulticastGroup(multicastEndPoint.Address);
            BeginReceive();
        }        

        public void Stop()
        {
            started = false;
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
            byte[] data = null;
            IPEndPoint remoteEndPoint = null;
            try
            {
                data = client.EndReceive(ar, ref remoteEndPoint);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }            

            BeginReceive();
            
            if (data == null)
                return;

            var message = Message.Deserialize(data);                
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

        void BeginReceive()
        {
            if (started)
                client.BeginReceive(OnReceive, null);
        }
    }
}
