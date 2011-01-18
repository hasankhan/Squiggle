using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Squiggle.Chat.Services;
using System.Net;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat
{    
    class Chat: IChat
    {
        Dictionary<object, Buddy> buddies;
        IChatSession session;
        Func<object, Buddy> buddyResolver;

        public Chat(IChatSession session, Buddy buddy, Func<object, Buddy> buddyResolver) : this(session, Enumerable.Repeat(buddy, 1), buddyResolver) { }

        public Chat(IChatSession session, IEnumerable<Buddy> buddies, Func<object, Buddy> buddyResolver)
        {
            this.buddyResolver = buddyResolver;
            this.buddies = new Dictionary<object, Buddy>();
            foreach (Buddy buddy in buddies)
                this.buddies[buddy.ID] = buddy;
            this.session = session;
            session.MessageReceived += new EventHandler<Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs>(session_MessageReceived);
            session.UserTyping += new EventHandler<Squiggle.Chat.Services.Chat.Host.SessionEventArgs>(session_UserTyping);
            session.BuzzReceived += new EventHandler<Squiggle.Chat.Services.Chat.Host.SessionEventArgs>(session_BuzzReceived);
            session.TransferInvitationReceived += new EventHandler<Squiggle.Chat.Services.FileTransferInviteEventArgs>(session_TransferInvitationReceived);
            session.UserJoined += new EventHandler<Services.Chat.Host.SessionEventArgs>(session_UserJoined);
            session.UserLeft += new EventHandler<Services.Chat.Host.SessionEventArgs>(session_UserLeft);
            session.GroupChatStarted += new EventHandler(session_GroupChatStarted);
        }                  

        #region IChat Members

        public IEnumerable<Buddy> Buddies
        {
            get { return buddies.Values; }
        }

        public bool IsGroupChat
        {
            get { return session.IsGroupSession; }
        }

        public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<MessageFailedEventArgs> MessageFailed = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyJoined = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyLeft = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyTyping = delegate { };
        public event EventHandler<BuddyEventArgs> BuzzReceived = delegate { };
        public event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler GroupChatStarted = delegate { };

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
            ThreadPool.QueueUserWorkItem(_ =>
            {
                L(() => session.SendBuzz());
            });
        }

        public IFileTransfer SendFile(string name, Stream content)
        {
            if (IsGroupChat)
                throw new InvalidOperationException("Can not send a file in a group chat session.");

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

        public void Invite(Buddy buddy)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                L(() => session.Invite(new ChatEndPoint(buddy.ID.ToString(), buddy.ChatEndPoint)));
            });
        }

        #endregion

        void session_GroupChatStarted(object sender, EventArgs e)
        {
            foreach (ChatEndPoint user in session.RemoteUsers)
            {
                Buddy buddy = buddyResolver(user.ClientID);
                if (buddy != null)
                    buddies[user.ClientID] = buddy;
            }
            GroupChatStarted(this, EventArgs.Empty);
        }   

        void session_TransferInvitationReceived(object sender, Squiggle.Chat.Services.FileTransferInviteEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGetValue(e.Sender.ClientID, out buddy))
                TransferInvitationReceived(this, new FileTransferInviteEventArgs() { Sender = buddy, Invitation = e.Invitation });
        }

        void session_BuzzReceived(object sender, Squiggle.Chat.Services.Chat.Host.SessionEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGetValue(e.Sender.ClientID, out buddy))
                BuzzReceived(this, new BuddyEventArgs( buddy ));
        } 

        void session_UserTyping(object sender, Squiggle.Chat.Services.Chat.Host.SessionEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGetValue(e.Sender.ClientID, out buddy))
                BuddyTyping(this, new BuddyEventArgs( buddy ));
        }

        void session_UserLeft(object sender, Services.Chat.Host.SessionEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGetValue(e.Sender.ClientID, out buddy))
            {
                buddies.Remove(buddy.ID);
                BuddyLeft(this, new BuddyEventArgs(buddy));
            }
        }

        void session_UserJoined(object sender, Services.Chat.Host.SessionEventArgs e)
        {
            Buddy buddy = buddyResolver(e.Sender.ClientID);
            if (buddy != null)
            {
                buddies[buddy.ID] = buddy;
                BuddyJoined(this, new BuddyEventArgs( buddy ));
            }
        }   

        void session_MessageReceived(object sender, Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGetValue(e.Sender.ClientID, out buddy))
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
