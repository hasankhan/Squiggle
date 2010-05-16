using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services;
using System.IO;
using System.Threading;

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
        public event EventHandler<MessageFailedEventArgs> MessageFailed = delegate { };

        public void SendMessage(string message)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    session.SendMessage(message);
                }
                catch (Exception ex)
                {
                    MessageFailed(this, new MessageFailedEventArgs()
                    {
                        Message = message,
                        Exception = ex
                    });
                }
            });
        }

        public void Leave()
        {
            session.End();
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
