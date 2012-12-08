using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using Squiggle.Core.Chat;
using Squiggle.Core.Presence.Transport.Multicast;
using Squiggle.Utilities;
using Squiggle.Utilities.Net;
using Squiggle.Utilities.Net.Pipe;
using Squiggle.Utilities.Threading;
using Squiggle.Core.Presence.Transport.Multicast.Tcp;

namespace Squiggle.Core.Presence.Transport
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }

        public bool IsBroadcast
        {
            get { return Message.Recipient == null; }
        }
    }

    public class PresenceChannel
    {
        IMulticastService multicastService;
        IPEndPoint serviceEndPoint;
        PresenceHost host;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public Guid ChannelID { get; private set; }

        public PresenceChannel(IPEndPoint multicastEndPoint, IPEndPoint multicastReceiveEndPoint, IPEndPoint serviceEndPoint)
        {
            this.ChannelID = Guid.NewGuid();

            if (NetworkUtility.IsMulticast(multicastEndPoint.Address))
                this.multicastService = new UdpMulticastService(multicastReceiveEndPoint, multicastEndPoint);
            else
                this.multicastService = new TcpMulticastService(multicastReceiveEndPoint, multicastEndPoint);
            
            this.serviceEndPoint = serviceEndPoint;
        }        

        public void Start()
        {
            host = new PresenceHost(serviceEndPoint);
            host.MessageReceived += host_MessageReceived;
            host.Start();

            multicastService.MessageReceived += multicastService_MessageReceived;
            multicastService.Start();
        }

        public void Stop()
        {
            host.Dispose();
            host = null;

            multicastService.MessageReceived -= multicastService_MessageReceived;
            multicastService.Stop();
        }

        public void MulticastMessage(Message message)
        {
            message.Recipient = null;
            message.ChannelID = ChannelID;
            multicastService.SendMessage(message);
        }

        public void SendMessage(Message message)
        {
            message.ChannelID = ChannelID;

            ExceptionMonster.EatTheException(() =>
            {
                host.Send(message);
            }, "sending presence message to " + message.Recipient);
        }

        void multicastService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            OnMessageReceived(e);
        }

        void host_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            OnMessageReceived(e);
        }

        void OnMessageReceived(MessageReceivedEventArgs e)
        {
            Async.Invoke(() =>
            {
                if (!e.Message.ChannelID.Equals(ChannelID))
                    MessageReceived(this, e);
            });
        }
    }
}
