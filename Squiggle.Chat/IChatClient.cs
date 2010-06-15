using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat
{
    public class ChatStartedEventArgs : EventArgs
    {
        public Buddy Buddy { get; set; }
        public IChat Chat { get; set; }
    }

    public class BuddyOnlineEventArgs : BuddyEventArgs
    {
        public bool Discovered { get; set; }
    }

    public interface IChatClient: IDisposable
    {
        event EventHandler<ChatStartedEventArgs> ChatStarted;
        event EventHandler<BuddyOnlineEventArgs> BuddyOnline;
        event EventHandler<BuddyEventArgs> BuddyOffline;
        event EventHandler<BuddyEventArgs> BuddyUpdated;

        Buddy CurrentUser{get; }
        IEnumerable<Buddy> Buddies { get; }
        bool LoggedIn { get; }

        IChat StartChat(Buddy buddy);
        void Login(string username, string displayMessage, Dictionary<string, string> properties);
        void Logout();
    }
}
