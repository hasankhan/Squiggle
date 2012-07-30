using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Threading;
using Squiggle.Core.Chat;

namespace Squiggle.Chat
{
    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public Buddy Sender { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
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
        public ActivitySession Session { get; set; }
        public IDictionary<string, string> Metadata { get; set; }

        public ActivityInvitationReceivedEventArgs(Buddy buddy) : base(buddy) { }
    }

    public interface IChat
    {
        IEnumerable<Buddy> Buddies { get; }
        bool IsGroupChat { get; }
        bool EnableLogging { get; set; }
        
        event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;
        event EventHandler<BuddyEventArgs> BuddyJoined;        
        event EventHandler<BuddyEventArgs> BuddyLeft;
        event EventHandler<BuddyEventArgs> BuzzReceived;
        event EventHandler<BuddyEventArgs> BuddyTyping;
        event EventHandler<MessageFailedEventArgs> MessageFailed;
        event EventHandler<ActivityInvitationReceivedEventArgs> ActivityInvitationReceived;
        event EventHandler GroupChatStarted;

        void NotifyTyping();
        void SendBuzz();
        void SendMessage(string fontName, int fontSize, Color color, FontStyle style, string message);
        ActivitySession CreateActivitySession();
        void Leave();

        void Invite(Buddy buddy);
    }
}
