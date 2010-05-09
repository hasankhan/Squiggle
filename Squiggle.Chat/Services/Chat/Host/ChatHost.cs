using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Service;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;

namespace Squiggle.Chat.Services.Chat.Host
{
    class MessageReceivedEventArgs : EventArgs
    {
        public string User { get; set; }
        public string Message { get; set; }
    }

    class ChatHost: IChatHost
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        #region IChatHost Members

        public void ReceiveMessage(string user, string message)
        {
            var remoteEndPoint = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            MessageReceived(this, new MessageReceivedEventArgs() { User = remoteEndPoint.Address, Message = message });
        }

        #endregion
    }
}
