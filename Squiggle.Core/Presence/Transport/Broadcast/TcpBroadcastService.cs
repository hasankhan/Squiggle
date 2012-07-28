using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Squiggle.Core.Presence.Transport.Broadcast.MultcastService;
using Squiggle.Utilities;
using Squiggle.Utilities.Net.Pipe;
using Squiggle.Core.Presence.Transport.Broadcast.MultcastService.Messages;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Core.Presence.Transport.Broadcast
{
    class TcpBroadcastService: IBroadcastService
    {
        IPEndPoint endpoint;
        IPEndPoint server;
        MulticastHost mcastHost;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public TcpBroadcastService(IPEndPoint endpoint, IPEndPoint server)
        {
            this.endpoint = endpoint;
            this.server = server;
        }

        public void SendMessage(Message message)
        {
            var msg = new MulticastMessage() { Sender = endpoint, Data = SerializationHelper.Serialize(message) };
            if (mcastHost != null)
                mcastHost.Send(server, msg);
        }

        public void Start()
        {
            mcastHost = new MulticastHost(endpoint);
            mcastHost.MessageReceived += new EventHandler<MultcastService.MessageReceivedEventArgs>(mcastHost_MessageReceived);
            mcastHost.Start();
            mcastHost.Send(server, new RegisterMessage() { Sender = endpoint });
        }

        void mcastHost_MessageReceived(object sender, MultcastService.MessageReceivedEventArgs e)
        {
            if (e.Message is MulticastMessage)
            {
                var message = (MulticastMessage)e.Message;
                SerializationHelper.Deserialize<Squiggle.Core.Presence.Transport.Message>(message.Data, presenceMessage =>
                {
                    MessageReceived(this, new MessageReceivedEventArgs() { Message = presenceMessage });
                }, "multicast message");
            }
        }

        public void Stop()
        {
            if (mcastHost != null)
            {
                mcastHost.Send(server, new UnregisterMessage() { Sender = endpoint });
                mcastHost.Dispose();
                mcastHost = null;
            }
        }
    }
}
