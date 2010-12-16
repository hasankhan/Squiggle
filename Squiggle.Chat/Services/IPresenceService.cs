using System;
using System.Collections.Generic;
using Squiggle.Chat.Services.Presence;

namespace Squiggle.Chat.Services
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
