using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using Squiggle.Core.Chat;
using Squiggle.Core.Presence.Transport.Broadcast;
using Squiggle.Utilities;
using Squiggle.Utilities.Net;
using Squiggle.Utilities.Net.Pipe;

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
        IBroadcastService broadcastService;
        IPEndPoint serviceEndPoint;
        PresenceHost host;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public Guid ChannelID { get; private set; }

        public PresenceChannel(IPEndPoint broadcastEndPoint, IPEndPoint broadcastReceiveEndPoint, IPEndPoint serviceEndPoint)
        {
            this.ChannelID = Guid.NewGuid();

            if (NetworkUtility.IsMulticast(broadcastEndPoint.Address))
                this.broadcastService = new UdpBroadcastService(broadcastReceiveEndPoint, broadcastEndPoint);
            else
                this.broadcastService = new TcpBroadcastService(broadcastReceiveEndPoint, broadcastEndPoint);
            
            this.serviceEndPoint = serviceEndPoint;
        }        

        public void Start()
        {
            host = new PresenceHost(serviceEndPoint);
            host.MessageReceived += new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
            host.Start();

            broadcastService.MessageReceived += new EventHandler<MessageReceivedEventArgs>(broadcastService_MessageReceived);
            broadcastService.Start();
        }

        public void Stop()
        {
            host.Dispose();
            host = null;

            broadcastService.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(broadcastService_MessageReceived);
            broadcastService.Stop();
        }

        public void BroadcastMessage(Message message)
        {
            message.Recipient = null;
            message.ChannelID = ChannelID;
            broadcastService.SendMessage(message);
        }

        public void SendMessage(Message message)
        {
            message.ChannelID = ChannelID;

            ExceptionMonster.EatTheException(() =>
            {
                host.Send(message);
            }, "sending presence message to " + message.Recipient);
        }

        void broadcastService_MessageReceived(object sender, MessageReceivedEventArgs e)
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
