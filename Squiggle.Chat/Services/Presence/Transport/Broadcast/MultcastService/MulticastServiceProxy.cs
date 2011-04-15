using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Utilities;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Squiggle.Chat.Services.Presence.Transport.Broadcast.MultcastService
{
    class MulticastServiceProxy: ProxyBase<IMulticastService>, IMulticastService
    {
        Binding binding;
        EndpointAddress address;
        IMulticastServiceCallback callback;

        public MulticastServiceProxy(Binding binding, EndpointAddress remoteAddress, IMulticastServiceCallback callback)
        {
            this.binding = binding;
            this.address = remoteAddress;
            this.callback = callback;
        }

        protected override ClientBase<IMulticastService> CreateProxy()
        {
            return new InnerProxy(new InstanceContext(callback), binding, address);
        }

        public void RegisterClient()
        {
            EnsureProxy(p => p.RegisterClient());
        }

        public void UnRegisterClient()
        {
            EnsureProxy(p => p.UnRegisterClient());
        }

        public void ForwardMessage(Message message)
        {
            EnsureProxy(p => p.ForwardMessage(message));
        }

        class InnerProxy: ClientBase<IMulticastService>, IMulticastService
        {
            public InnerProxy(InstanceContext callback, Binding binding, EndpointAddress remoteAddress):base(callback, binding, remoteAddress)
            {

            }

            public void RegisterClient()
            {
                Channel.RegisterClient();
            }

            public void UnRegisterClient()
            {
                Channel.UnRegisterClient();
            }

            public void ForwardMessage(Message message)
            {
                Channel.ForwardMessage(message);
            }
        }        
    }
}
