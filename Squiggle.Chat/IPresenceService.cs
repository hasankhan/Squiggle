using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    class UserEventArgs: EventArgs
    {
        public UserInfo User {get; set; }
    }
    
    interface IPresenceService
    {
        event EventHandler<UserEventArgs> UserOnline;
        event EventHandler<UserEventArgs> UserOffline;
        event EventHandler<UserEventArgs> UserUpdated;
        IEnumerable<UserInfo> Users { get; }
        
        void ChangeStatus(UserStatus status);
        void ChangeName(string name);

        void Login(string name);
        void Logout();
    }
}
