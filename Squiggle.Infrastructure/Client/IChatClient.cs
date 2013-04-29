using System;
using System.Collections.Generic;
using System.Linq;
using Squiggle.Core.Presence;

namespace Squiggle.Client
{    
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
        void Login(LoginOptions options);
        void Logout();
    }
}
