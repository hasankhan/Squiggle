using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport.Host
{
    class PresenceHostProxy: IPresenceHost
    {
        InnerProxy proxy;
        Binding binding;
        EndpointAddress address;

        public PresenceHostProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.address = remoteAddress;
            EnsureProxy();
        }

         void EnsureProxy()
         {
             if (proxy == null || proxy.State == CommunicationState.Faulted)
             {
                 if (proxy == null)
                     proxy = new InnerProxy(binding, address);
                 else
                 {
                     try
                     {
                         proxy.Close();
                     }
                     catch (Exception ex)
                     {
                         Trace.WriteLine(ex.Message);
                         proxy.Abort();
                     }
                     finally
                     {
                         proxy = new InnerProxy(binding, address);
                     }
                 }
             }
         }       

        public UserInfo GetUserInfo()
        {
            EnsureProxy();
            return proxy.GetUserInfo(); 
        }

        public void ReceiveMessage(IPEndPoint sender, byte[] message)
        {
            EnsureProxy();
            proxy.ReceiveMessage(sender, message);
        }

        class InnerProxy : ClientBase<IPresenceHost>, IPresenceHost
        {
            public InnerProxy()
            {
            }

            public InnerProxy(string endpointConfigurationName)
                :
                    base(endpointConfigurationName)
            {
            }

            public InnerProxy(string endpointConfigurationName, string remoteAddress)
                :
                    base(endpointConfigurationName, remoteAddress)
            {
            }

            public InnerProxy(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress)
                :
                    base(endpointConfigurationName, remoteAddress)
            {
            }

            public InnerProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
                :
                    base(binding, remoteAddress)
            {
            }

            public UserInfo GetUserInfo()
            {
                return base.Channel.GetUserInfo(); 
            }

            public void ReceiveMessage(IPEndPoint sender, byte[] message)
            {
                base.Channel.ReceiveMessage(sender, message);
            }
        }
    }
}
