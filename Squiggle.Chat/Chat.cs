using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Squiggle.Chat.Services;

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
            session.BuzzReceived += new EventHandler<Squiggle.Chat.Services.Chat.Host.UserEventArgs>(session_BuzzReceived);
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
        public event EventHandler<BuddyEventArgs> BuzzReceived = delegate { };
        public event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived = delegate { };

        public void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    session.SendMessage(fontName, fontSize, color, fontStyle, message);
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
                L(() => session.NotifyTyping());
            });
        }

        public void SendBuzz()
        {
            L(()=>session.SendBuzz());
        }

        public IFileTransfer SendFile(string name, Stream content)
        {
            try
            {
                return session.SendFile(name, content);            
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }

        public void Leave()
        {
            L(()=>session.End());
        }

        #endregion

        void session_TransferInvitationReceived(object sender, Squiggle.Chat.Services.FileTransferInviteEventArgs e)
        {
            TransferInvitationReceived(this, new FileTransferInviteEventArgs() { Sender = buddy, Invitation = e.Invitation });
        }

        void session_BuzzReceived(object sender, Squiggle.Chat.Services.Chat.Host.UserEventArgs e)
        {
            BuzzReceived(this, new BuddyEventArgs() { Buddy = buddy });
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
                                                                       FontStyle = e.FontStyle,                                                                       
                                                                       Message = e.Message});
        }

        bool L(Action action)
        {
            bool success = true;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                success = false;
                Trace.WriteLine(ex.Message);
            }
            return success;
        }
    }
}
