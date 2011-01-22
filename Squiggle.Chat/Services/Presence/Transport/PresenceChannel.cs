using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using Squiggle.Chat.Services.Presence.Transport.Host;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat.Services.Presence.Transport
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public SquiggleEndPoint Recipient { get; set; }
        public SquiggleEndPoint Sender { get; set; }
        public Message Message { get; set; }

        public bool IsBroadcast
        {
            get { return Recipient == null; }
        }
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
        PresenceHost presenceHost;
        ServiceHost serviceHost;

        public event EventHandler<UserInfoRequestedEventArgs> UserInfoRequested = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public Guid ChannelID { get { return channelID; } }

        public PresenceChannel(IPEndPoint multicastEndPoint, IPEndPoint serviceEndPoint)
        {
            udpReceiveEndPoint = new IPEndPoint(serviceEndPoint.Address, multicastEndPoint.Port);
            this.multicastEndPoint = multicastEndPoint;
            this.serviceEndPoint = serviceEndPoint;
            this.presenceHost = new PresenceHost();
            this.presenceHost.UserInfoRequested += new EventHandler<UserInfoRequestedEventArgs>(presenceHost_UserInfoRequested);
            presenceHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(presenceHost_MessageReceived);
            this.presenceHosts = new Dictionary<IPEndPoint, IPresenceHost>();
        }      

        public void Start()
        {
            started = true;
            client = new UdpClient();
            client.DontFragment = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(udpReceiveEndPoint);
            client.JoinMulticastGroup(multicastEndPoint.Address);
           
            serviceHost = new ServiceHost(presenceHost);
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

        public void SendMessage(Message message, SquiggleEndPoint localEndPoint, SquiggleEndPoint presenceEndPoint)
        {
            IPresenceHost host = GetPresenceHost(presenceEndPoint.Address);
            try
            {
                host.ReceivePresenceMessage(localEndPoint, presenceEndPoint, message.Serialize());
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could send message to " + presenceEndPoint + " due to exception: " + ex.Message);
            }
        }

        public UserInfo GetUserInfo(SquiggleEndPoint user)
        {
            IPresenceHost host = GetPresenceHost(user.Address);
            UserInfo info = null;
            try
            {
                info = host.GetUserInfo(user);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could not get user info of " + user + " due to exception: " + ex.Message);
            }
            return info;
        }

        void presenceHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                OnMessageReceived(e.Sender, e.Recipient, e.Message);
            });
        }

        void presenceHost_UserInfoRequested(object sender, UserInfoRequestedEventArgs e)
        {
            UserInfoRequested(this, e);
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
                    OnMessageReceived(new SquiggleEndPoint(message.ClientID, message.PresenceEndPoint), null, message);
                });

            BeginReceive();
        }

        void OnMessageReceived(SquiggleEndPoint sender, SquiggleEndPoint recipient, Message message)
        {
            if (!message.ChannelID.Equals(channelID) && message.PresenceEndPoint != null)
            {
                var args = new MessageReceivedEventArgs()
                {
                    Recipient = recipient,
                    Message = message,
                    Sender = sender
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
