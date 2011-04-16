using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ServiceModel;
using Squiggle.Chat;
using Squiggle.Chat.Services.Presence.Transport.Broadcast.MultcastService;

namespace Squiggle.Multicast
{
    class MulticastService
    {
        MulticastHost mcastHost;
        ServiceHost serviceHost;

        public MulticastService()
        {
            mcastHost = new MulticastHost();
        }

        public void Start(IPEndPoint endPoint)
        {
            serviceHost = new ServiceHost(mcastHost);
            var address = new Uri("net.tcp://" + endPoint.ToString() + "/squigglemulticast");
            var binding = BindingHelper.CreateBinding();
            serviceHost.AddServiceEndpoint(typeof(IMulticastService), binding, address);

            serviceHost.Open();
        }

        public void Stop()
        {
            serviceHost.Close();
            mcastHost.Reset();
        }
    }
}
