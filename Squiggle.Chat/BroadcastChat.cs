using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    class BroadcastChat: IChat
    {
        IEnumerable<IChat> chatSessions;

        public BroadcastChat(IEnumerable<IChat> chatSessions)
        {
            this.chatSessions = chatSessions;
            foreach (var session in chatSessions)
            {
                session.MessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(session_MessageReceived);
                session.MessageFailed += new EventHandler<MessageFailedEventArgs>(session_MessageFailed);
                session.BuzzReceived += new EventHandler<BuddyEventArgs>(session_BuzzReceived);
                session.BuddyTyping += new EventHandler<BuddyEventArgs>(session_BuddyTyping);
            }
        }

        public IEnumerable<Buddy> Buddies
        {
            get { return chatSessions.SelectMany(session => session.Buddies); }
        }

        public bool IsGroupChat
        {
            get { return true; }
        }

        public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyJoined = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyLeft = delegate { };
        public event EventHandler<BuddyEventArgs> BuzzReceived = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyTyping = delegate { };
        public event EventHandler<MessageFailedEventArgs> MessageFailed = delegate { };
        public event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler GroupChatStarted = delegate { };

        public void NotifyTyping()
        {
            chatSessions.ForEach(s => s.NotifyTyping());
        }

        public void SendBuzz()
        {
            chatSessions.ForEach(s => s.SendBuzz());
        }

        public void SendMessage(string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle style, string message)
        {
            chatSessions.ForEach(s => s.SendMessage(fontName, fontSize, color, style, message));
        }

        public IFileTransfer SendFile(string name, System.IO.Stream content)
        {
            throw new InvalidOperationException("Can not send a file in a broadcast chat session.");
        }

        public void Leave()
        {
            chatSessions.ForEach(s=>s.Leave());
        }

        public void Invite(Buddy buddy)
        {
            throw new InvalidOperationException("Can not invite buddies in a broadcast chat session.");
        }

        void session_BuddyTyping(object sender, BuddyEventArgs e)
        {
            BuddyTyping(sender, e);
        }

        void session_BuzzReceived(object sender, BuddyEventArgs e)
        {
            BuzzReceived(sender, e);
        }

        void session_MessageFailed(object sender, MessageFailedEventArgs e)
        {
            MessageFailed(sender, e);
        }

        void session_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            MessageReceived(sender, e);
        }
    }
}
