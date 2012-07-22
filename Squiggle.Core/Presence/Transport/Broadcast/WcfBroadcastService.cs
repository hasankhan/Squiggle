using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Squiggle.Core.Presence.Transport.Broadcast.MultcastService;
using Squiggle.Utilities;
using Squiggle.Utilities.Net.Wcf;

namespace Squiggle.Core.Presence.Transport.Broadcast
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class WcfBroadcastService: IBroadcastService, IMulticastServiceCallback
    {
        Binding binding;
        Uri address;
        MulticastServiceProxy proxy;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public WcfBroadcastService(IPEndPoint server)
        {
            this.address = new Uri("net.tcp://" + server.ToString() + "/" + ServiceNames.MulticastService);
            this.binding = WcfConfig.CreateBinding();
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
            });
        }
    }
}
