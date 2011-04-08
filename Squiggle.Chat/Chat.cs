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
using Squiggle.Utilities;
using Squiggle.History;
using Squiggle.History.DAL;

namespace Squiggle.Chat
{    
    class Chat: IChat
    {
        Dictionary<object, Buddy> buddies;
        IChatSession session;
        Func<object, Buddy> buddyResolver;
        Buddy self;

        public Chat(IChatSession session, Buddy self, Buddy buddy, Func<object, Buddy> buddyResolver) : this(session, self, Enumerable.Repeat(buddy, 1), buddyResolver) { }

        public Chat(IChatSession session, Buddy self, IEnumerable<Buddy> buddies, Func<object, Buddy> buddyResolver)
        {
            this.self = self;
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

        public bool EnableLogging { get; set; }

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
            Async.Invoke(() =>
            {
                Exception ex;
                if (!ExceptionMonster.EatTheException(()=>
                                    {
                                        session.SendMessage(fontName, fontSize, color, fontStyle, message);                    
                                    }, "sending chat message", out ex))
                    MessageFailed(this, new MessageFailedEventArgs()
                    {
                        Message = message,
                        Exception = ex
                    });
                LogHistory(EventType.Message, self, message);
            });
        }

        public void NotifyTyping()
        {
            Async.Invoke(() => 
            {
                L(() => session.NotifyTyping(), "sending typing message");
            });
        }

        public void SendBuzz()
        {
            Async.Invoke(() =>
            {
                L(() => session.SendBuzz(), "sending buzz");
                LogHistory(EventType.Buzz, self);
            });
        }

        public IFileTransfer SendFile(string name, Stream content)
        {
            if (IsGroupChat)
                throw new InvalidOperationException("Can not send a file in a group chat session.");

            return ExceptionMonster.EatTheException(() =>
            {
                var transfer = session.SendFile(name, content);
                LogHistory(EventType.Transfer, self, name);
                return transfer;
            }, "sending file request");
        }

        public void Leave()
        {
            L(()=>session.End(), "leaving chat");
            LogHistory(EventType.Left, self);
        }

        public void Invite(Buddy buddy)
        {
            Async.Invoke(() =>
            {
                var endpoint = new SquiggleEndPoint(buddy.ID.ToString(), buddy.ChatEndPoint);
                L(() => session.Invite(endpoint), "sending chat invite to " + endpoint);
            });
        }

        #endregion

        void session_GroupChatStarted(object sender, EventArgs e)
        {
            foreach (SquiggleEndPoint user in session.RemoteUsers)
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
            {
                TransferInvitationReceived(this, new FileTransferInviteEventArgs() { Sender = buddy, Invitation = e.Invitation });
                LogHistory(EventType.Transfer, buddy);
            }
        }

        void session_BuzzReceived(object sender, Squiggle.Chat.Services.Chat.Host.SessionEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGetValue(e.Sender.ClientID, out buddy))
            {
                BuzzReceived(this, new BuddyEventArgs(buddy));
                LogHistory(EventType.Buzz, buddy);
            }
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
                LogHistory(EventType.Left, buddy);
            }
        }

        void session_UserJoined(object sender, Services.Chat.Host.SessionEventArgs e)
        {
            Buddy buddy = buddyResolver(e.Sender.ClientID);
            if (buddy != null)
            {
                buddies[buddy.ID] = buddy;
                BuddyJoined(this, new BuddyEventArgs( buddy ));
                LogHistory(EventType.Joined, buddy);
            }
        }   

        void session_MessageReceived(object sender, Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGetValue(e.Sender.ClientID, out buddy))
            {
                MessageReceived(this, new ChatMessageReceivedEventArgs()
                {
                    Sender = buddy,
                    FontName = e.FontName,
                    FontSize = e.FontSize,
                    Color = e.Color,
                    FontStyle = e.FontStyle,
                    Message = e.Message
                });
                LogHistory(EventType.Message, buddy, e.Message);
            }
        }

        bool sessionLogged;
        void LogHistory(EventType eventType, Buddy sender, string data = null)
        {
            DoHistoryAction(manager=>
            {
                if (!sessionLogged)
                    LogSessionStart();
                manager.AddSessionEvent(session.ID, DateTime.Now, eventType, new Guid(sender.ID.ToString()), sender.DisplayName, buddies.Values.Select(b => new Guid(b.ID.ToString())), data);
            });
        }

        void LogSessionStart()
        {
            DoHistoryAction(manager =>
            {
                sessionLogged = true;
                Buddy primaryBuddy = Buddies.FirstOrDefault();
                var newSession = Session.CreateSession(session.ID, new Guid(primaryBuddy.ID.ToString()), primaryBuddy.DisplayName, DateTime.Now);
                manager.AddSession(newSession, Buddies.Select(b => Participant.CreateParticipant(Guid.NewGuid(), new Guid(b.ID.ToString()), b.DisplayName)));
            });
        }

        void DoHistoryAction(Action<HistoryManager> action)
        {
            if (EnableLogging)
                L(() => action(new HistoryManager()), "logging history.");
        }

        bool L(Action action, string description)
        {
            return ExceptionMonster.EatTheException(action, description);
        }        
    }
}
