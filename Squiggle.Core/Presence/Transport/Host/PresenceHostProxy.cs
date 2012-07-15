using System;
using System.Diagnostics;
using System.Net;
using Squiggle.Utilities;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Core.Chat;

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

        public void ReceivePresenceMessage(SquiggleEndPoint recepient, byte[] message)
        {
            EnsureProxy(p=>
            {
                p.ReceivePresenceMessage(recepient, message);
            });
        }

        class InnerProxy : ClientBase<IPresenceHost>, IPresenceHost
        {            
            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                :
                    base(binding, remoteAddress)
            {
            }

            public void ReceivePresenceMessage(SquiggleEndPoint recepient, byte[] message)
            {
                base.Channel.ReceivePresenceMessage(recepient, message);
            }
        }
    }
}
