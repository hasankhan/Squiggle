using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat.Service
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
            MessageReceived(this, new MessageReceivedEventArgs() { User = user, Message = message });
        }

        #endregion
    }
}
