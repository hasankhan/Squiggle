using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Utilities;
using Squiggle.Utilities.Net.Wcf;

namespace Squiggle.Core.Chat.Transport.Host
{
    public class ChatHostProxy: ProxyBase<IChatHost>, IChatHost
    {
        Binding binding;
        EndpointAddress address;

        public ChatHostProxy(IPEndPoint remoteEndPoint)
        {
            Uri uri = CreateServiceUri(remoteEndPoint.ToString());
            this.binding = WcfConfig.CreateBinding(); ;
#if !DEBUG
            this.binding.SendTimeout = TimeSpan.FromSeconds(5);
#endif
            this.address = new EndpointAddress(uri);
        }

        protected override ClientBase<IChatHost> CreateProxy()
        {
            return new InnerProxy(binding, address);
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/" + ServiceNames.ChatService);
            return uri;
        }

        #region IChatHost Members

        public void ReceiveChatMessage(byte[] message)
        {
            EnsureProxy(p => p.ReceiveChatMessage(message));
        }

        #endregion

        class InnerProxy : ClientBase<IChatHost>, IChatHost
        {
            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                : base(binding, remoteAddress)
            {
            }

            #region IChatHost Members

            public void ReceiveChatMessage(byte[] message)
            {
                base.Channel.ReceiveChatMessage(message);
            }

            #endregion
        }
    }
}
