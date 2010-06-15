using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Squiggle.Chat.Services;
using System.Windows.Media;

namespace Squiggle.Chat
{    
    class Chat: IChat
    {
        Buddy buddy;
        IChatSession session;

        public Chat(IChatSession session, Buddy buddy)
        {
            this.buddy = buddy;
            this.session = session;
            session.MessageReceived += new EventHandler<Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs>(session_MessageReceived);
            session.UserTyping += new EventHandler<Squiggle.Chat.Services.Chat.Host.UserEventArgs>(session_UserTyping);
            session.TransferInvitationReceived += new EventHandler<Squiggle.Chat.Services.FileTransferInviteEventArgs>(session_TransferInvitationReceived);
        }        

        #region IChat Members

        public IEnumerable<Buddy> Buddies
        {
            get { return Enumerable.Repeat(buddy, 1); }
        }

        public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<MessageFailedEventArgs> MessageFailed = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyJoined = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyLeft = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyTyping = delegate { };
        public event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived = delegate { };

        public void SendMessage(string fontName, int fontSize, Color color, string message)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    session.SendMessage(fontName, fontSize, color, message);
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

        public void NotifyTyping()
        {
            ThreadPool.QueueUserWorkItem(_ => 
            {
                try
                {
                    session.NotifyTyping();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            });
        }

        public IFileTransfer SendFile(string name, Stream content)
        {
            return session.SendFile(name, content);            
        }

        public void Leave()
        {
            session.End();
        }

        #endregion

        void session_TransferInvitationReceived(object sender, Squiggle.Chat.Services.FileTransferInviteEventArgs e)
        {
            TransferInvitationReceived(this, new FileTransferInviteEventArgs() { Sender = buddy, Invitation = e.Invitation });
        }

        void session_UserTyping(object sender, Squiggle.Chat.Services.Chat.Host.UserEventArgs e)
        {
            BuddyTyping(this, new BuddyEventArgs() { Buddy = buddy });
        }                

        void session_MessageReceived(object sender, Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs e)
        {
            MessageReceived(this, new ChatMessageReceivedEventArgs() { Sender = buddy, 
                                                                       FontName = e.FontName,
                                                                       FontSize = e.FontSize,
                                                                       Color = e.Color,
                                                                       Message = e.Message});
        }
    }
}
