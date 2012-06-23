using System;
using System.Collections.Generic;
using System.Linq;
using Squiggle.Chat;
using System.Net;
using Squiggle.Core.Chat.Voice;
using Squiggle.Core.Presence;

namespace Squiggle.UI.ViewModel
{
    class DummyChatClient: IChatClient
    {
        Buddy self;

        public DummyChatClient()
        {
            self = new Buddy(this, new object(), null, new BuddyProperties());
            self.Status = UserStatus.Offline;
        }

        #region IChatClient Members

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler<BuddyOnlineEventArgs> BuddyOnline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyOffline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyUpdated = delegate { };

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

        public IVoiceChat ActiveVoiceChat
        {
            get { return null; }
        }

        public IChat StartChat(Buddy buddy)
        {
            throw new NotImplementedException();
        }

        public void Login(string username, BuddyProperties properties)
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
