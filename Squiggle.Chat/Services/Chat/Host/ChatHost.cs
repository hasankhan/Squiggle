using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;

namespace Squiggle.Chat.Services.Chat.Host
{
    public  class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint User { get; set; }
        public string Message { get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class ChatHost: IChatHost
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        #region IChatHost Members

        public void ReceiveMessage(IPEndPoint user, string message)
        {
            var remoteEndPoint = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            MessageReceived(this, new MessageReceivedEventArgs() { User = user, Message = message });
        }

        #endregion
    }
}
