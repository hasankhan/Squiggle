using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Squiggle.Core.Chat;

namespace Squiggle.Chat
{
    public class BroadcastChat: IChat
    {
        List<IChat> chatSessions;

        public BroadcastChat(IEnumerable<IChat> chatSessions)
        {
            this.chatSessions = new List<IChat>();
            foreach (var session in chatSessions)
                AddSession(session);
        }        

        public IEnumerable<Buddy> Buddies
        {
            get 
            { 
                lock (chatSessions)
                    return chatSessions.SelectMany(session => session.Buddies).ToList(); 
            }
        }

        public IEnumerable<IChat> ChatSessions
        {
            get
            {
                lock (chatSessions)
                    return chatSessions.ToList();
            }
        }

        public bool IsGroupChat
        {
            get { return true; }
        }

        public bool EnableLogging
        {
            get
            {
                lock (chatSessions)
                    return chatSessions.All(cs => cs.EnableLogging);
            }
            set
            {
                lock (chatSessions)
                    foreach (IChat session in chatSessions)
                        session.EnableLogging = value;
            }
        }

        public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyJoined = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyLeft = delegate { };
        public event EventHandler<BuddyEventArgs> BuzzReceived = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyTyping = delegate { };
        public event EventHandler<MessageFailedEventArgs> MessageFailed = delegate { };
        public event EventHandler<ActivityInvitationReceivedEventArgs> AppInvitationReceived = delegate { };
        public event EventHandler GroupChatStarted = delegate { };

        public void NotifyTyping()
        {
            lock(chatSessions)
                chatSessions.ForEach(s => s.NotifyTyping());
        }

        public void SendBuzz()
        {
            lock (chatSessions)
                chatSessions.ForEach(s => s.SendBuzz());
        }

        public void SendMessage(string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle style, string message)
        {
            lock (chatSessions)
                chatSessions.ForEach(s => s.SendMessage(fontName, fontSize, color, style, message));
        }

        public ActivitySession CreateAppSession()
        {
            throw new InvalidOperationException("Can not start app session in broadcast chat.");
        }

        public void Leave()
        {
            lock (chatSessions)
                chatSessions.ForEach(s=>s.Leave());
        }

        public void Invite(Buddy buddy)
        {
            throw new InvalidOperationException("Can not invite buddies in a broadcast chat.");
        }

        public void AddSession(IChat session)
        {
            lock (chatSessions)
            {
                this.chatSessions.Add(session);
                session.MessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(session_MessageReceived);
                session.MessageFailed += new EventHandler<MessageFailedEventArgs>(session_MessageFailed);
                session.BuzzReceived += new EventHandler<BuddyEventArgs>(session_BuzzReceived);
                session.BuddyTyping += new EventHandler<BuddyEventArgs>(session_BuddyTyping);
            }
        }

        public void RemoveSession(IChat session)
        {
            lock (chatSessions)
            {
                this.chatSessions.Remove(session);
                session.MessageReceived -= new EventHandler<ChatMessageReceivedEventArgs>(session_MessageReceived);
                session.MessageFailed -= new EventHandler<MessageFailedEventArgs>(session_MessageFailed);
                session.BuzzReceived -= new EventHandler<BuddyEventArgs>(session_BuzzReceived);
                session.BuddyTyping -= new EventHandler<BuddyEventArgs>(session_BuddyTyping);
            }
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
