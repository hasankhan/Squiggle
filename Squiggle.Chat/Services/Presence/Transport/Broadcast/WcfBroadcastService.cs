using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Squiggle.Chat.Services.Presence.Transport.Broadcast.MultcastService;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Squiggle.Chat.Services.Presence.Transport.Broadcast
{
    class WcfBroadcastService: IBroadcastService, IMulticastServiceCallback
    {
        Binding binding;
        Uri address;
        MulticastServiceProxy proxy;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public WcfBroadcastService(IPEndPoint server)
        {
            this.address = new Uri("net.tcp://" + server.ToString() + "/squigglemulticast");
            this.binding = BindingHelper.CreateBinding();
        }

        public void SendMessage(Message message)
        {
            if (proxy != null)
                proxy.ForwardMessage(message);
        }

        public void Start()
        {
            this.proxy = new MulticastServiceProxy(binding, new EndpointAddress(address), this);
            proxy.RegisterClient();
        }

        public void Stop()
        {
            if (proxy != null)
            {
                proxy.UnRegisterClient();
                proxy.Dispose();
                proxy = null;
            }
        }

        public void MessageForwarded(Message message)
        {
            MessageReceived(this, new MessageReceivedEventArgs()
            {
                Message = message,
                Sender = new SquiggleEndPoint(message.ClientID, message.PresenceEndPoint),
                Recipient = null
            });
        }
    }
}
