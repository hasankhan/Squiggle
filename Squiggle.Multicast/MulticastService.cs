using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using Squiggle.Core;
using Squiggle.Core.Presence.Transport.Broadcast.MultcastService;
using Squiggle.Utilities;
using Squiggle.Utilities.Net.Wcf;

namespace Squiggle.Multicast
{
    class MulticastService: WcfHost
    {
        MulticastHost mcastHost;
        IPEndPoint endPoint;

        public MulticastService(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            mcastHost = new MulticastHost();
        }

        protected override void OnStop()
        {
            base.OnStop();

            mcastHost.Reset();
        }

        protected override ServiceHost CreateHost()
        {
            var serviceHost = new ServiceHost(mcastHost);

            var address = new Uri("net.tcp://" + endPoint.ToString() + "/" + ServiceNames.MulticastService);
            var binding = WcfConfig.CreateBinding();
            serviceHost.AddServiceEndpoint(typeof(IMulticastService), binding, address);

            return serviceHost;
        }
    }
}
