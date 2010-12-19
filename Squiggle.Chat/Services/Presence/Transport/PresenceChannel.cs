using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using Squiggle.Chat.Services.Presence.Transport.Host;

namespace Squiggle.Chat.Services.Presence.Transport
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint Sender { get; set; }
        public Message Message { get; set; }
    }

    public class PresenceChannel
    {
        UdpClient client;
        IPEndPoint udpReceiveEndPoint;
        IPEndPoint multicastEndPoint;
        IPEndPoint serviceEndPoint;
        Dictionary<IPEndPoint, IPresenceHost> presenceHosts;

        Guid channelID = Guid.NewGuid();
        bool started;
        PresenceHost host;
        ServiceHost serviceHost;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public Guid ChannelID { get { return channelID; } }

        public PresenceChannel(IPEndPoint multicastEndPoint, IPEndPoint serviceEndPoint)
        {
            udpReceiveEndPoint = new IPEndPoint(serviceEndPoint.Address, multicastEndPoint.Port);
            this.multicastEndPoint = multicastEndPoint;
            this.serviceEndPoint = serviceEndPoint;
            this.host = new PresenceHost();
            host.MessageReceived += new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
            this.presenceHosts = new Dictionary<IPEndPoint, IPresenceHost>();
        }

        public UserInfo UserInfo
        {
            get { return host.UserInfo; }
            set { host.UserInfo = value; }
        }

        public void Start()
        {
            started = true;
            client = new UdpClient();
            client.DontFragment = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(udpReceiveEndPoint);
            client.JoinMulticastGroup(multicastEndPoint.Address);
           
            serviceHost = new ServiceHost(host);
            var address = CreateServiceUri(serviceEndPoint.ToString());
            var binding = BindingHelper.CreateBinding();
            serviceHost.AddServiceEndpoint(typeof(IPresenceHost), binding, address);
            serviceHost.Open();

            BeginReceive();
        }        

        public void Stop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }

            started = false;
            client.Close();
        }

        public void SendMessage(Message message)
        {
            message.ChannelID = channelID;
            byte[] data = message.Serialize();
            try
            {
                client.Send(data, data.Length, multicastEndPoint);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could not send broadcast message due to exception: " + ex.Message);
            }
        }

        public void SendMessage(Message message, IPEndPoint presenceEndPoint)
        {
            IPresenceHost host = GetPresenceHost(presenceEndPoint);
            try
            {
                host.ReceiveMessage(serviceEndPoint, message.Serialize());
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could send message to " + presenceEndPoint + " due to exception: " + ex.Message);
            }
        }

        public UserInfo GetUserInfo(IPEndPoint endPoint)
        {
            IPresenceHost host = GetPresenceHost(endPoint);
            UserInfo info = null;
            try
            {
                info = host.GetUserInfo();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could not get user info of " + endPoint + " due to exception: " + ex.Message);
            }
            return info;
        }

        void host_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                OnMessageReceived(e.Sender, e.Message);
            });
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

            if (data != null)
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var message = Message.Deserialize(data);
                    OnMessageReceived(remoteEndPoint, message);
                });

            BeginReceive();
        }

        private void OnMessageReceived(IPEndPoint remoteEndPoint, Message message)
        {
            if (!message.ChannelID.Equals(channelID) && message.PresenceEndPoint != null)
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
                try
                {
                    client.BeginReceive(OnReceive, null);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Could not receive data on presence channel due to exception: " + ex.Message);
                }
        }        

        IPresenceHost GetPresenceHost(IPEndPoint endPoint)
        {
            IPresenceHost host;
            if (!presenceHosts.TryGetValue(endPoint, out host))
            {
                Uri uri = CreateServiceUri(endPoint.ToString());
                var binding = BindingHelper.CreateBinding();
                host = new PresenceHostProxy(binding, new EndpointAddress(uri));
                presenceHosts[endPoint] = host;
            }
            return host;
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/squigglepresence");
            return uri;
        }
    }
}
