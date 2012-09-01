using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Squiggle.Core.Chat;
using Squiggle.Core.Chat.Activity;

namespace Squiggle.Client
{
    public class BuddyEventArgs : EventArgs
    {
        public BuddyEventArgs() { }

        public BuddyEventArgs(IBuddy buddy)
        {
            this.Buddy = buddy;
        }

        public IBuddy Buddy { get; set; }
    }

    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public Guid Id { get; set; }
        public IBuddy Sender { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; }
    }

    public class ChatMessageUpdatedEventArgs: EventArgs
    {
        public Guid Id { get; set; }
        public IBuddy Sender { get; set; }
        public string Message { get; set; }
    }

    public class MessageFailedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public class ActivityInvitationReceivedEventArgs : BuddyEventArgs
    {
        public Guid ActivityId { get; set; }
        public IActivityExecutor Executor { get; set; }
        public IDictionary<string, string> Metadata { get; set; }

        public ActivityInvitationReceivedEventArgs(IBuddy buddy) : base(buddy) { }
    }

    public interface IChat
    {
        IEnumerable<IBuddy> Buddies { get; }
        bool IsGroupChat { get; }
        bool EnableLogging { get; set; }
        
        event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;
        event EventHandler<ChatMessageUpdatedEventArgs> MessageUpdated;
        event EventHandler<BuddyEventArgs> BuddyJoined;        
        event EventHandler<BuddyEventArgs> BuddyLeft;
        event EventHandler<BuddyEventArgs> BuzzReceived;
        event EventHandler<BuddyEventArgs> BuddyTyping;
        event EventHandler<MessageFailedEventArgs> MessageFailed;
        event EventHandler<ActivityInvitationReceivedEventArgs> ActivityInvitationReceived;
        event EventHandler GroupChatStarted;

        void NotifyTyping();
        void SendBuzz();
        void SendMessage(Guid id, string fontName, int fontSize, Color color, FontStyle style, string message);
        IActivityExecutor CreateActivity();
        void Leave();

        void Invite(IBuddy buddy);
    }
}
