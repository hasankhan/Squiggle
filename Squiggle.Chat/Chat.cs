using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services;

namespace Squiggle.Chat
{
    class Chat: IChat
    {
        Buddy buddy;
        IChatSession session;

        #region IChat Members

        public IEnumerable<Buddy> Buddies
        {
            get { return Enumerable.Repeat(buddy, 1); }
        }

        public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyJoined = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyLeft = delegate { };

        public void SendMessage(string message)
        {
            session.SendMessage(message);
        }

        #endregion

        public Chat(IChatSession session, Buddy buddy)
        {
            this.buddy = buddy;
            this.session = session;
            session.MessageReceived += new EventHandler<Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs>(session_MessageReceived);
        }                

        void session_MessageReceived(object sender, Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs e)
        {
            MessageReceived(this, new ChatMessageReceivedEventArgs() { Sender = buddy, Message = e.Message});
        }
    }
}
