using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Squiggle.Core;
using System.Net;
using Squiggle.Core.Chat;
using Squiggle.Utilities;
using Squiggle.History;
using Squiggle.History.DAL;
using System.Windows.Threading;
using Squiggle.Core.Chat.FileTransfer;
using Squiggle.Core.Chat.Voice;

namespace Squiggle.Chat
{    
    class Chat: IChat
    {
        IChatSession session;
        ChatBuddies buddies;
        Buddy self;

        public Chat(IChatSession session, Buddy self, Buddy buddy, Func<object, Buddy> buddyResolver) : this(session, self, Enumerable.Repeat(buddy, 1), buddyResolver) { }

        public Chat(IChatSession session, Buddy self, IEnumerable<Buddy> buddies, Func<object, Buddy> buddyResolver)
        {
            this.self = self;
            
            this.buddies = new ChatBuddies(session, buddyResolver, buddies);
            this.buddies.BuddyJoined += new EventHandler<BuddyEventArgs>(buddies_BuddyJoined);
            this.buddies.BuddyLeft += new EventHandler<BuddyEventArgs>(buddies_BuddyLeft);
            this.buddies.GroupChatStarted += new EventHandler(buddies_GroupChatStarted);

            this.session = session;
            session.MessageReceived += new EventHandler<Squiggle.Core.Chat.Transport.Host.MessageReceivedEventArgs>(session_MessageReceived);
            session.UserTyping += new EventHandler<Squiggle.Core.Chat.Transport.Host.SessionEventArgs>(session_UserTyping);
            session.BuzzReceived += new EventHandler<Squiggle.Core.Chat.Transport.Host.SessionEventArgs>(session_BuzzReceived);
            session.TransferInvitationReceived += new EventHandler<Squiggle.Core.Chat.FileTransferInviteEventArgs>(session_TransferInvitationReceived);
            session.VoiceChatInvitationReceived += new EventHandler<VoiceChatInvitationReceivedEventArgs>(session_VoiceChatInvitationReceived);
        }        

        #region IChat Members

        public IEnumerable<Buddy> Buddies
        {
            get { return buddies; }
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
        public event EventHandler<VoiceChatInviteEventArgs> VoiceChatInvitationReceived = delegate { };
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

        public IVoiceChat StartVoiceChat(Dispatcher dispatcher)
        {
            if (IsGroupChat)
                throw new InvalidOperationException("Can not start voice chat in group chat session.");

            return ExceptionMonster.EatTheException(() =>
            {
                var chat = session.StartVoiceChat(dispatcher);
                LogHistory(EventType.Voice, self);
                return chat;
            }, "sending voice chat invite");
        }

        public void Leave()
        {
            Async.Invoke(() =>
            {
                L(() => session.End(), "leaving chat");
                LogHistory(EventType.Left, self);
            });
        }

        public void Invite(Buddy buddy)
        {
            Async.Invoke(() =>
            {
                var endpoint = new SquiggleEndPoint(buddy.Id, buddy.ChatEndPoint);
                L(() => session.Invite(endpoint), "sending chat invite to " + endpoint);
            });
        }

        #endregion

        void buddies_GroupChatStarted(object sender, EventArgs e)
        {
            GroupChatStarted(this, EventArgs.Empty);
        }

        void session_TransferInvitationReceived(object sender, Squiggle.Core.Chat.FileTransferInviteEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGet(e.Sender.ClientID, out buddy))
            {
                TransferInvitationReceived(this, new FileTransferInviteEventArgs() { Sender = buddy, Invitation = e.Invitation });
                LogHistory(EventType.Transfer, buddy);
            }
        }


        void session_VoiceChatInvitationReceived(object sender, VoiceChatInvitationReceivedEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGet(e.Sender.ClientID, out buddy))
            {
                VoiceChatInvitationReceived(this, new VoiceChatInviteEventArgs() { Sender = buddy, Invitation = e.Invitation });
                LogHistory(EventType.Voice, buddy);
            }
        }                  

        void session_BuzzReceived(object sender, Squiggle.Core.Chat.Transport.Host.SessionEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGet(e.Sender.ClientID, out buddy))
            {
                BuzzReceived(this, new BuddyEventArgs(buddy));
                LogHistory(EventType.Buzz, buddy);
            }
        } 

        void session_UserTyping(object sender, Squiggle.Core.Chat.Transport.Host.SessionEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGet(e.Sender.ClientID, out buddy))
                BuddyTyping(this, new BuddyEventArgs( buddy ));
        }

        void buddies_BuddyLeft(object sender, BuddyEventArgs e)
        {
            BuddyLeft(this, e);
            LogHistory(EventType.Left, e.Buddy);
        }

        void buddies_BuddyJoined(object sender, BuddyEventArgs e)
        {
            BuddyJoined(this, e);
            LogHistory(EventType.Joined, e.Buddy);
        }

        void session_MessageReceived(object sender, Squiggle.Core.Chat.Transport.Host.MessageReceivedEventArgs e)
        {
            Buddy buddy;
            if (buddies.TryGet(e.Sender.ClientID, out buddy))
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
                manager.AddSessionEvent(session.ID, DateTime.Now, eventType, new Guid(sender.Id), sender.DisplayName, buddies.Select(b => new Guid(b.Id)), data);
            });
        }

        void LogSessionStart()
        {
            DoHistoryAction(manager =>
            {
                sessionLogged = true;
                Buddy primaryBuddy = Buddies.FirstOrDefault();
                var newSession = Session.CreateSession(session.ID, new Guid(primaryBuddy.Id), primaryBuddy.DisplayName, DateTime.Now);
                manager.AddSession(newSession, Buddies.Select(b => Participant.CreateParticipant(Guid.NewGuid(), new Guid(b.Id), b.DisplayName)));
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
