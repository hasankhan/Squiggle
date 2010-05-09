using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence;

namespace Squiggle.Chat
{
    class UserEventArgs: EventArgs
    {
        public UserInfo User {get; set; }
    }
    
    interface IPresenceService: IDisposable
    {
        event EventHandler<UserEventArgs> UserOnline;
        event EventHandler<UserEventArgs> UserOffline;
        IEnumerable<UserInfo> Users { get; }
        
        void Login(string name);
        void Logout();
    }
}
