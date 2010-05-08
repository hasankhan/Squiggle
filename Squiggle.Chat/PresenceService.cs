using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    class PresenceService: IPresenceService
    {
        #region IPresenceService Members

        public event EventHandler<UserEventArgs> UserOnline;

        public event EventHandler<UserEventArgs> UserOffline;

        public event EventHandler<UserEventArgs> UserUpdated;

        public IEnumerable<UserInfo> Users
        {
            get { throw new NotImplementedException(); }
        }

        public void ChangeStatus(UserStatus status)
        {
            throw new NotImplementedException();
        }

        public void ChangeName(string name)
        {
            throw new NotImplementedException();
        }

        public void Login(string name)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
