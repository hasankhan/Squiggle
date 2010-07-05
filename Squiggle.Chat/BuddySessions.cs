using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services;

namespace Squiggle.Chat
{
    class BuddySessions
    {
        Dictionary<Buddy, IChatSession> sessions = new Dictionary<Buddy, IChatSession>();

        public event EventHandler<Services.Chat.Host.MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<Services.Chat.Host.UserEventArgs> BuzzReceived = delegate { };
        public event EventHandler<Services.Chat.Host.UserEventArgs> UserTyping = delegate { };

        public void Add(Buddy buddy, IChatSession session)
        {
            Remove(buddy);
            sessions[buddy] = session;
            session.BuzzReceived += new EventHandler<Services.Chat.Host.UserEventArgs>(session_BuzzReceived);
            session.MessageReceived += new EventHandler<Services.Chat.Host.MessageReceivedEventArgs>(session_MessageReceived);
            session.UserTyping += new EventHandler<Services.Chat.Host.UserEventArgs>(session_UserTyping);
        }

        public IEnumerable<Buddy> Buddies
        {
            get { return sessions.Keys; }
        }

        public void Remove(Buddy buddy)
        {
            IChatSession session;
            sessions.TryGetValue(buddy, out session);
            if (session != null)
            {
                session.BuzzReceived -= new EventHandler<Services.Chat.Host.UserEventArgs>(session_BuzzReceived);
                session.MessageReceived -= new EventHandler<Services.Chat.Host.MessageReceivedEventArgs>(session_MessageReceived);
                session.UserTyping -= new EventHandler<Services.Chat.Host.UserEventArgs>(session_UserTyping);
            }
        }

        void session_UserTyping(object sender, Services.Chat.Host.UserEventArgs e)
        {
            UserTyping(sender, e);
        }

        void session_MessageReceived(object sender, Services.Chat.Host.MessageReceivedEventArgs e)
        {
            MessageReceived(sender, e);
        }

        void session_BuzzReceived(object sender, Services.Chat.Host.UserEventArgs e)
        {
            BuzzReceived(sender, e);
        }

        public void SendBuzz()
        {
            foreach (IChatSession session in sessions.Values)
                session.SendBuzz();
        }

        public void SendMessage(string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle fontStyle, string message)
        {
            foreach (IChatSession session in sessions.Values)
                session.SendMessage(fontName, fontSize, color, fontStyle, message);
        }

        public void NotifyTyping()
        {
            foreach (IChatSession session in sessions.Values)
                session.NotifyTyping();
        }        

        public void End()
        {
            foreach (IChatSession session in sessions.Values)
                session.End();
        }
    }
}
