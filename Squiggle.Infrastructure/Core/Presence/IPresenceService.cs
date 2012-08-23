using System;
using System.Collections.Generic;

namespace Squiggle.Core.Presence
{
    public class UserEventArgs : EventArgs
    {
        public IUserInfo User { get; set; }
    }

    public class UserOnlineEventArgs : UserEventArgs
    {
        public bool Discovered { get; set; }
    }

    public interface IPresenceService: IDisposable
    {
        event EventHandler<UserOnlineEventArgs> UserOnline;
        event EventHandler<UserEventArgs> UserOffline;
        event EventHandler<UserEventArgs> UserUpdated;

        IEnumerable<IUserInfo> Users { get; }
        
        void Login(string displayName, IBuddyProperties properties);
        void Update(string displayName, IBuddyProperties properties, UserStatus status);
        void Logout();
    }
}
