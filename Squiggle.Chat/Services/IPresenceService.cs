using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence;
using System.Net;

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
        
        void Login(string name, string displayMessage);
        void Update(string userFriendlyName, string displayMessage, UserStatus status);
        void Logout();
    }
}
