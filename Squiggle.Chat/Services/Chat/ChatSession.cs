using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Chat.Host;
using System.Net;

namespace Squiggle.Chat.Services.Chat
{
    class ChatSession: IChatSession
    {
        IChatHost remoteHost;
        IPEndPoint localUser;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public ChatSession(ChatHost localHost, IChatHost remoteHost, IPEndPoint localUser, IPEndPoint remoteUser)
        {
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            RemoteUser = remoteUser;
            localHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
        }

        void host_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            MessageReceived(this, e);
        }

        #region IChatSession Members

        public void SendMessage(string message)
        {
            remoteHost.ReceiveMessage(localUser, message);
        }

        #endregion


        #region IChatSession Members


        public IPEndPoint RemoteUser { get; set; }
        

        #endregion
    }
}
