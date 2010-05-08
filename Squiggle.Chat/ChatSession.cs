using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Service;

namespace Squiggle.Chat
{
    class ChatSession: IChatSession
    {
        IChatHost remoteHost;
        string localUser;
        string remoteUser;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public ChatSession(ChatHost localHost, IChatHost remoteHost, string localUser, string remoteUser)
        {
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            this.remoteUser = remoteUser;
            localHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
        }

        void host_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.User == remoteUser)
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
