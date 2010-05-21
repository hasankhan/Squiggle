using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.CodeDom.Compiler;
using System.Net;

namespace Squiggle.Chat.Services.Chat.Host
{
    [GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class ChatHostProxy : ClientBase<IChatHost>, IChatHost
    {
        public ChatHostProxy()
        {
        }

        public ChatHostProxy(string endpointConfigurationName)
            :
                base(endpointConfigurationName)
        {
        }

        public ChatHostProxy(string endpointConfigurationName, string remoteAddress)
            :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public ChatHostProxy(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress)
            :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public ChatHostProxy(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
            :
                base(binding, remoteAddress)
        {
        }

        #region IChatHost Members

        public void UserIsTyping(IPEndPoint user)
        {
            base.Channel.UserIsTyping(user);
        }

        public void ReceiveMessage(IPEndPoint user, string message)
        {
            base.Channel.ReceiveMessage(user, message);
        }

        #endregion
    }
}
