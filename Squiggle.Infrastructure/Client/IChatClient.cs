using System;
using System.Collections.Generic;
using System.Linq;
using Squiggle.Core.Presence;

namespace Squiggle.Client
{
    public class ChatStartedEventArgs : EventArgs
    {
        public IBuddy Buddy
        {
            get { return Buddies.FirstOrDefault(); }
        }
        public IEnumerable<IBuddy> Buddies { get; set; }
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
        event EventHandler LoggedIn;
        event EventHandler LoggedOut;

        ISelfBuddy CurrentUser {get; }
        IEnumerable<IBuddy> Buddies { get; }
        bool IsLoggedIn { get; }
        bool EnableLogging { get; set; }

        IChat StartChat(IBuddy buddy);
        void Login(ChatClientOptions options);
        void Logout();
    }
}
