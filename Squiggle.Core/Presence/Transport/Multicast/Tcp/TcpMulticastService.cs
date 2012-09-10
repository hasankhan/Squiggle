using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Squiggle.Utilities;
using Squiggle.Utilities.Net.Pipe;
using Squiggle.Core.Presence.Transport.Multicast.Tcp.Messages;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Core.Presence.Transport.Multicast.Tcp
{
    class TcpMulticastService: IMulticastService
    {
        IPEndPoint endpoint;
        IPEndPoint server;
        MulticastHost mcastHost;

        public event EventHandler<Squiggle.Core.Presence.Transport.MessageReceivedEventArgs> MessageReceived = delegate { };

        public TcpMulticastService(IPEndPoint endpoint, IPEndPoint server)
        {
            this.endpoint = endpoint;
            this.server = server;
        }

        public void SendMessage(Squiggle.Core.Presence.Transport.Message message)
        {
            var msg = new MulticastMessage() { Sender = endpoint, Data = SerializationHelper.Serialize(message) };
            if (mcastHost != null)
                mcastHost.Send(server, msg);
        }

        public void Start()
        {
            mcastHost = new MulticastHost(endpoint);
            mcastHost.MessageReceived += new EventHandler<Tcp.MessageReceivedEventArgs>(mcastHost_MessageReceived);
            mcastHost.Start();
            mcastHost.Send(server, new RegisterMessage() { Sender = endpoint });
        }

        void mcastHost_MessageReceived(object sender, Tcp.MessageReceivedEventArgs e)
        {
            if (e.Message is MulticastMessage)
            {
                var message = (MulticastMessage)e.Message;
                SerializationHelper.Deserialize<Squiggle.Core.Presence.Transport.Message>(message.Data, presenceMessage =>
                {
                    MessageReceived(this, new Squiggle.Core.Presence.Transport.MessageReceivedEventArgs() { Message = presenceMessage });
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
