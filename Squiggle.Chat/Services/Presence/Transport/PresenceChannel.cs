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
        IBroadcastService broadcastService;
        IPEndPoint serviceEndPoint;
        Dictionary<IPEndPoint, IPresenceHost> presenceHosts;

        PresenceHost presenceHost;
        ServiceHost serviceHost;

        public event EventHandler<UserInfoRequestedEventArgs> UserInfoRequested = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public Guid ChannelID { get; private set; }

        public PresenceChannel(IPEndPoint multicastEndPoint, IPEndPoint serviceEndPoint)
        {
            this.ChannelID = Guid.NewGuid();
            var udpReceiveEndPoint = new IPEndPoint(serviceEndPoint.Address, multicastEndPoint.Port);
            this.broadcastService = new UdpBroadcastService(udpReceiveEndPoint, multicastEndPoint);
            this.serviceEndPoint = serviceEndPoint;
            this.presenceHost = new PresenceHost();
            this.presenceHost.UserInfoRequested += new EventHandler<UserInfoRequestedEventArgs>(presenceHost_UserInfoRequested);
            presenceHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(presenceHost_MessageReceived);
            this.presenceHosts = new Dictionary<IPEndPoint, IPresenceHost>();
        }      

        public void Start()
        {
            broadcastService.MessageReceived += new EventHandler<MessageReceivedEventArgs>(broadcastService_MessageReceived);
            broadcastService.Start();

            serviceHost = new ServiceHost(presenceHost);
            var address = CreateServiceUri(serviceEndPoint.ToString());
            var binding = BindingHelper.CreateBinding();
            serviceHost.AddServiceEndpoint(typeof(IPresenceHost), binding, address);
            serviceHost.Open();
        }     

        public void Stop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }

            broadcastService.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(broadcastService_MessageReceived);
            broadcastService.Stop();
        }

        public void SendMessage(Message message)
        {
            message.ChannelID = ChannelID;
            broadcastService.SendMessage(message);
        }

        public void SendMessage(Message message, SquiggleEndPoint localEndPoint, SquiggleEndPoint presenceEndPoint)
        {
            message.ChannelID = ChannelID;
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

        void broadcastService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Async.Invoke(() =>
            {
                OnMessageReceived(e.Sender, e.Recipient, e.Message);
            });
        }   

        void presenceHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Async.Invoke(() =>
            {
                OnMessageReceived(e.Sender, e.Recipient, e.Message);
            });
        }

        void presenceHost_UserInfoRequested(object sender, UserInfoRequestedEventArgs e)
        {
            UserInfoRequested(this, e);
        }

        void OnMessageReceived(SquiggleEndPoint sender, SquiggleEndPoint recipient, Message message)
        {
            if (message.IsValid && !message.ChannelID.Equals(ChannelID))
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
