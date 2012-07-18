using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Core.Presence;
using System.Net;
using System.Drawing;
using Squiggle.Utilities;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Core.Chat;
using Squiggle.Core;
using System.Collections.Generic;

namespace Squiggle.Bridge
{
    class BridgeHostProxy: ProxyBase<IBridgeHost>, IBridgeHost
    {
        Binding binding;
        EndpointAddress address;

        public BridgeHostProxy(Binding binding, EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.address = remoteAddress;
        }

        protected override ClientBase<IBridgeHost> CreateProxy()
        {
            return new InnerProxy(binding, address);
        }

        #region IBridgeHost

        public void ForwardPresenceMessage(SquiggleEndPoint recipient, byte[] message, IPEndPoint bridgeEndPoint)
        {
            EnsureProxy(p => p.ForwardPresenceMessage(recipient, message, bridgeEndPoint));
        }
        
        #endregion        

        #region IChatHost

        public void ReceiveChatMessage(byte[] message)
        {
            EnsureProxy(p => p.ReceiveChatMessage(message));
        }

        #endregion        

        class InnerProxy : ClientBase<IBridgeHost>, IBridgeHost
        {          
            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                : base(binding, remoteAddress)
            {
            }

            #region IBridgeHost

            public void ForwardPresenceMessage(SquiggleEndPoint recipient, byte[] message, IPEndPoint bridgeEndPoint)
            {
                this.Channel.ForwardPresenceMessage(recipient, message, bridgeEndPoint);
            }

            #endregion

            #region IChatHost

            public void ReceiveChatMessage(byte[] message)
            {
                this.Channel.ReceiveChatMessage(message);
            }

            #endregion
        }

    }
}
