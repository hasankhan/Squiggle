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
        MessagePipe pipe;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public Guid ChannelID { get; private set; }

        public PresenceChannel(IPEndPoint multicastEndPoint, IPEndPoint serviceEndPoint)
        {
            this.ChannelID = Guid.NewGuid();
            if (NetworkUtility.IsMulticast(multicastEndPoint.Address))
            {
                var udpReceiveEndPoint = new IPEndPoint(serviceEndPoint.Address, multicastEndPoint.Port);
                this.broadcastService = new UdpBroadcastService(udpReceiveEndPoint, multicastEndPoint);
            }
            else
                this.broadcastService = new WcfBroadcastService(multicastEndPoint);
            
            this.serviceEndPoint = serviceEndPoint;
        }        

        public void Start()
        {
            pipe = new MessagePipe(serviceEndPoint);
            pipe.MessageReceived += new EventHandler<Utilities.Net.Pipe.MessageReceivedEventArgs>(pipe_MessageReceived);
            pipe.Open();

            broadcastService.MessageReceived += new EventHandler<MessageReceivedEventArgs>(broadcastService_MessageReceived);
            broadcastService.Start();
        }

        public void Stop()
        {
            pipe.Dispose();
            pipe = null;

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
                byte[] data = message.Serialize();
                pipe.Send(message.Recipient.Address, data);
            }, "sending presence message to " + message.Recipient);
        }

        void broadcastService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Async.Invoke(() =>
            {
                OnMessageReceived(e.Message);
            });
        }

        void pipe_MessageReceived(object sender, Utilities.Net.Pipe.MessageReceivedEventArgs e)
        {
            Async.Invoke(() =>
            {
                var msg = Message.Deserialize(e.Message);
                OnMessageReceived(msg);
            });
        }

        void OnMessageReceived(Message message)
        {
            if (!message.ChannelID.Equals(ChannelID))
            {
                var args = new MessageReceivedEventArgs(){ Message = message };
                MessageReceived(this, args);
            }
        }      
    }
}
