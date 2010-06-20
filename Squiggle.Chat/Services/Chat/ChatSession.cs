using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Chat.Host;
using System.Net;
using System.IO;
using System.Drawing;

namespace Squiggle.Chat.Services.Chat
{
    class ChatSession: IChatSession
    {
        IChatHost remoteHost;
        IPEndPoint localUser;
        ChatHost localHost;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler<UserEventArgs> UserTyping = delegate { };
        public event EventHandler<UserEventArgs> BuzzReceived = delegate { };

        public event EventHandler SessionEnded = delegate { };

        public IPEndPoint RemoteUser { get; set; }

        public ChatSession(ChatHost localHost, IChatHost remoteHost, IPEndPoint localUser, IPEndPoint remoteUser)
        {
            this.localHost = localHost;
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            RemoteUser = remoteUser;
            localHost.TransferInvitationReceived += new EventHandler<TransferInvitationReceivedEventArgs>(localHost_TransferInvitationReceived);
            localHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
            localHost.UserTyping += new EventHandler<UserEventArgs>(localHost_UserTyping);
            localHost.BuzzReceived += new EventHandler<UserEventArgs>(localHost_BuzzReceived);
        }

        void localHost_TransferInvitationReceived(object sender, TransferInvitationReceivedEventArgs e)
        {
            if (IsRemoteUser(e.User))
            {
                IFileTransfer invitation = new FileTransfer(remoteHost, localHost, localUser, e.Name, e.Size, e.ID);
                TransferInvitationReceived(this, new FileTransferInviteEventArgs() { Invitation = invitation });
            }
        }

        void localHost_UserTyping(object sender, UserEventArgs e)
        {
            if (IsRemoteUser(e.User))
                UserTyping(this, e);
        }

        void localHost_BuzzReceived(object sender, UserEventArgs e)
        {
            if (IsRemoteUser(e.User))
                BuzzReceived(this, e);
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

        public void SendBuzz()
        {
            remoteHost.Buzz(localUser);
        }

        public void NotifyTyping()
        {
            remoteHost.UserIsTyping(localUser);
        }

        public IFileTransfer SendFile(string name, Stream content)
        {
            long size = content.Length;
            var transfer = new FileTransfer(remoteHost, localHost, localUser, name, (int)size, content);
            transfer.Start();
            return transfer;
        }

        public void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            remoteHost.ReceiveMessage(localUser, fontName, fontSize, color, fontStyle, message);
        }

        public void End()
        {
            SessionEnded(this, EventArgs.Empty);
        }
    }
}
