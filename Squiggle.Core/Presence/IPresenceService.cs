using System;
using System.Collections.Generic;

namespace Squiggle.Core.Presence
{
    public class UserOnlineEventArgs : UserEventArgs
    {
        public bool Discovered { get; set; }
    }

    public interface IPresenceService: IDisposable
    {
        event EventHandler<UserOnlineEventArgs> UserOnline;
        event EventHandler<UserEventArgs> UserOffline;
        event EventHandler<UserEventArgs> UserUpdated;

        IEnumerable<UserInfo> Users { get; }
        
        void Login(string displayName, BuddyProperties properties);
        void Update(string displayName, BuddyProperties properties, UserStatus status);
        void Logout();
    }
}
