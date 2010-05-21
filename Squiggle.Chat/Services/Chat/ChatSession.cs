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
        public event EventHandler<UserEventArgs> UserTyping = delegate { };
        public event EventHandler SessionEnded = delegate { };

        public IPEndPoint RemoteUser { get; set; }


        public ChatSession(ChatHost localHost, IChatHost remoteHost, IPEndPoint localUser, IPEndPoint remoteUser)
        {
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            RemoteUser = remoteUser;
            localHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
            localHost.UserTyping += new EventHandler<UserEventArgs>(localHost_UserTyping);
        }

        void localHost_UserTyping(object sender, UserEventArgs e)
        {
            if (IsRemoteUser(e.User))
                UserTyping(this, e);
        }

        private bool IsRemoteUser(IPEndPoint iPEndPoint)
        {
            return iPEndPoint.Equals(RemoteUser);
        }

        void host_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (IsRemoteUser(e.User))
                MessageReceived(this, e);
        }

        public void NotifyTyping()
        {
            remoteHost.UserIsTyping(localUser);
        }

        public void SendMessage(string message)
        {
            remoteHost.ReceiveMessage(localUser, message);
        }

        public void End()
        {
            SessionEnded(this, EventArgs.Empty);
        }
    }
}
