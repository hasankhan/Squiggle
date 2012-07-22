using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Core.Chat;
using Squiggle.Utilities;
using Squiggle.Utilities.Net.Wcf;

namespace Squiggle.Core.Presence.Transport.Host
{
    class PresenceHostProxy: ProxyBase<IPresenceHost>, IPresenceHost
    {
        Binding binding;
        EndpointAddress address;

        public PresenceHostProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.address = remoteAddress;
        }

        protected override ClientBase<IPresenceHost> CreateProxy()
        {
            return new InnerProxy(binding, address);
        }

        public void ReceivePresenceMessage(byte[] message)
        {
            EnsureProxy(p=>
            {
                p.ReceivePresenceMessage(message);
            });
        }

        class InnerProxy : ClientBase<IPresenceHost>, IPresenceHost
        {            
            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                :
                    base(binding, remoteAddress)
            {
            }

            public void ReceivePresenceMessage(byte[] message)
            {
                base.Channel.ReceivePresenceMessage(message);
            }
        }
    }
}
