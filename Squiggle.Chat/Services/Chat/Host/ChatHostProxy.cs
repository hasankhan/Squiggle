using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.CodeDom.Compiler;
using System.Net;
using System.Diagnostics;

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
            EnsureConnection();
            base.Channel.UserIsTyping(user);
            Trace.WriteLine("Sending typing notification to: " + user.ToString());
        }        

        public void ReceiveMessage(IPEndPoint user, string message)
        {
            EnsureConnection();
            base.Channel.ReceiveMessage(user, message);
            Trace.WriteLine("Sending message to:" + user.ToString() + ", message = " + message);
        }

        #endregion

        void EnsureConnection()
        {
            if (State == CommunicationState.Faulted || State == CommunicationState.Closed)
            {
                this.Close();
                this.Open();
            }
        }
    }
}
