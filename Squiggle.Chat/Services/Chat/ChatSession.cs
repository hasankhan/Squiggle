using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Service;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Chat.Services.Chat
{
    class ChatSession: IChatSession
    {
        IChatHost remoteHost;
        string localUser;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public ChatSession(ChatHost localHost, IChatHost remoteHost, string localUser)
        {
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            localHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
        }

        void host_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            MessageReceived(this, e);
        }

        #region IChatSession Members

        public void SendMessage(string message)
        {
            remoteHost.ReceiveMessage(this.localUser, message);
        }

        #endregion
    }
}
