using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.CodeDom.Compiler;
using System.Net;
using System.Diagnostics;
using System.ServiceModel.Channels;

namespace Squiggle.Chat.Services.Chat.Host
{
    [GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public class ChatHostProxy: IChatHost
    {
        InnerProxy proxy;
        Binding binding;
        EndpointAddress address;

        public ChatHostProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.address = remoteAddress;
            EnsureProxy();
        }

         void EnsureProxy()
         {
             if (proxy == null || proxy.State == CommunicationState.Faulted)
             {
                 if (proxy != null)
                     proxy.Abort();
                 proxy = new InnerProxy(binding, address);
             }
         }       

        #region IChatHost Members

        public void UserIsTyping(IPEndPoint user)
        {
            EnsureProxy();
            proxy.UserIsTyping(user);
        }

        public void ReceiveMessage(IPEndPoint user, string message)
        {
            EnsureProxy();
            proxy.ReceiveMessage(user, message);
        }

        #endregion

        class InnerProxy : ClientBase<IChatHost>, IChatHost
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

            #region IChatHost Members

            public void UserIsTyping(IPEndPoint user)
            {
                base.Channel.UserIsTyping(user);
                Trace.WriteLine("Sending typing notification to: " + user.ToString());
            }

            public void ReceiveMessage(IPEndPoint user, string message)
            {
                base.Channel.ReceiveMessage(user, message);
                Trace.WriteLine("Sending message to:" + user.ToString() + ", message = " + message);
            }

            #endregion
        }
    }
}
