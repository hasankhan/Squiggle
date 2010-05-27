using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;

namespace Squiggle.UI
{
    class DummyChatClient: IChatClient
    {
        Buddy self;

        public DummyChatClient()
        {
            self = new Buddy(this, new object());
            self.Status = UserStatus.Offline;
        }

        #region IChatClient Members

        public event EventHandler<ChatStartedEventArgs> ChatStarted;

        public event EventHandler<BuddyOnlineEventArgs> BuddyOnline;

        public event EventHandler<BuddyEventArgs> BuddyOffline;

        public event EventHandler<BuddyEventArgs> BuddyUpdated;

        public Buddy CurrentUser
        {
            get { return self; }
        }

        public IEnumerable<Buddy> Buddies
        {
            get { return Enumerable.Empty<Buddy>(); }
        }

        public bool LoggedIn
        {
            get { return false; }
        }

        public IChat StartChat(Buddy buddy)
        {
            throw new NotImplementedException();
        }

        public void Login(string username)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
