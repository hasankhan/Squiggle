using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Squiggle.Chat
{
    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public Buddy Sender { get; set; }
        public string Message { get; set; }
    }

    public class MessageFailedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public interface IChat
    {
        IEnumerable<Buddy> Buddies { get; }

        event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;
        event EventHandler<BuddyEventArgs> BuddyJoined;
        event EventHandler<BuddyEventArgs> BuddyLeft;
        event EventHandler<MessageFailedEventArgs> MessageFailed;

        void SendMessage(string Message);
        void Leave();
    }
}
