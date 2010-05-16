using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence;
using System.Net;

namespace Squiggle.Chat
{
    interface IPresenceService: IDisposable
    {
        event EventHandler<UserEventArgs> UserOnline;
        event EventHandler<UserEventArgs> UserOffline;
        event EventHandler<UserEventArgs> UserUpdated;

        IEnumerable<UserInfo> Users { get; }
        
        void Login(string name, string displayMessage);
        void Update(string userFriendlyName, string displayMessage, UserStatus status);
        void Logout();
    }
}
