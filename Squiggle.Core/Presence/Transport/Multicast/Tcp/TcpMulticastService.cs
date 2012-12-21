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
        IPEndPoint localEndpoint;
        IPEndPoint serverEndpoint;
        MulticastHost mcastHost;

        public event EventHandler<Squiggle.Core.Presence.Transport.MessageReceivedEventArgs> MessageReceived = delegate { };

        public TcpMulticastService(IPEndPoint localEndpoint, IPEndPoint server)
        {
            this.localEndpoint = localEndpoint;
            this.serverEndpoint = server;
        }

        public void SendMessage(Squiggle.Core.Presence.Transport.Message message)
        {
            var msg = new MulticastMessage() { Sender = localEndpoint, Data = SerializationHelper.Serialize(message) };
            if (mcastHost != null)
                mcastHost.Send(serverEndpoint, msg);
        }

        public void Start()
        {
            mcastHost = new MulticastHost(localEndpoint);
            mcastHost.MessageReceived += mcastHost_MessageReceived;
            mcastHost.Start();
            mcastHost.Send(serverEndpoint, new RegisterMessage() { Sender = localEndpoint });
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
                mcastHost.Send(serverEndpoint, new UnregisterMessage() { Sender = localEndpoint });
                mcastHost.Dispose();
                mcastHost = null;
            }
        }
    }
}
