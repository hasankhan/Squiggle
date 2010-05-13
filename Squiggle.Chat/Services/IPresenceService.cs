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
        IEnumerable<UserInfo> Users { get; }
        
        void Login(string name);
        void Logout();
    }
}
