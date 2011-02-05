using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using Squiggle.Chat.Services.Presence.Transport.Host;
using Squiggle.Chat.Services.Chat;
using Squiggle.Utilities;

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

            ExceptionMonster.EatTheException(() =>
            {
                client.Send(data, data.Length, multicastEndPoint);
            }, "sending presence mcast message");
        }

        public void SendMessage(Message message, SquiggleEndPoint localEndPoint, SquiggleEndPoint presenceEndPoint)
        {
            IPresenceHost host = GetPresenceHost(presenceEndPoint.Address);

            ExceptionMonster.EatTheException(() =>
            {
                host.ReceivePresenceMessage(localEndPoint, presenceEndPoint, message.Serialize());
            }, "sending presence message to " + presenceEndPoint);
        }

        public UserInfo GetUserInfo(SquiggleEndPoint user)
        {
            IPresenceHost host = GetPresenceHost(user.Address);
            UserInfo info = null;

            ExceptionMonster.EatTheException(() =>
                {
                    info = host.GetUserInfo(user);
                }, "getting user info for " + user);
            
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

            ExceptionMonster.EatTheException(() =>
            {
                data = client.EndReceive(ar, ref remoteEndPoint);
            }, "receiving mcast presence message");

            if (data != null)
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var message = Message.Deserialize(data);
                    if (message.IsValid)
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
                ExceptionMonster.EatTheException(() =>
                    {
                        client.BeginReceive(OnReceive, null);

                    }, "receiving mcast data on presence channel");
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
