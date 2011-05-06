using System;
using System.Collections.Generic;

namespace Squiggle.Chat.Services.Presence
{
    class UserOnlineEventArgs : UserEventArgs
    {
        public bool Discovered { get; set; }
    }

    interface IPresenceService: IDisposable
    {
        event EventHandler<UserOnlineEventArgs> UserOnline;
        event EventHandler<UserEventArgs> UserOffline;
        event EventHandler<UserEventArgs> UserUpdated;

        IEnumerable<UserInfo> Users { get; }
        
        void Login(string name, BuddyProperties properties);
        void Update(string displayName, Dictionary<string, string> properties, UserStatus status);
        void Logout();
    }
}
