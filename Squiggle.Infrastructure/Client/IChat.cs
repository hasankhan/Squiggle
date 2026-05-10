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

        public IBuddy Buddy { get; set; } = null!;
    }

    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public Guid Id { get; set; }
        public IBuddy Sender { get; set; } = null!;
        public string FontName { get; set; } = null!;
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; } = null!;
    }

    public class ChatMessageUpdatedEventArgs: EventArgs
    {
        public Guid Id { get; set; }
        public IBuddy Sender { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    public class MessageFailedEventArgs : EventArgs
    {
        public string Message { get; set; } = null!;
        public Exception Exception { get; set; } = null!;
    }

    public class ActivityInvitationReceivedEventArgs : BuddyEventArgs
    {
        public Guid ActivityId { get; set; }
        public IActivityExecutor Executor { get; set; } = null!;
        public IDictionary<string, string> Metadata { get; set; } = null!;

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
        void UpdateMessage(Guid id, string message);
        IActivityExecutor CreateActivity(Guid activityId);
        void Leave();

        void Invite(IBuddy buddy);
    }
}
