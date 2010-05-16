using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Squiggle.Chat
{    
    public interface IChat
    {
        IEnumerable<Buddy> Buddies { get; }

        event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;
        event EventHandler<BuddyEventArgs> BuddyJoined;
        event EventHandler<BuddyEventArgs> BuddyLeft;
        event EventHandler<ErrorEventArgs> Error;

        void SendMessage(string Message);
        void Leave();
    }
}
