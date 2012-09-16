using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Squiggle.Client;
using Squiggle.Core.Presence;
using Squiggle.Core.Chat.Activity;
using Squiggle.Client.Activities;

namespace Squiggle.UI.ViewModel
{
    class DummyChatClient: IChatClient
    {
        DummySelfBuddy self;

        public DummyChatClient()
        {
            self = new DummySelfBuddy();
            self.Status = UserStatus.Offline;
        }

        #region IChatClient Members

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler<BuddyOnlineEventArgs> BuddyOnline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyOffline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyUpdated = delegate { };

        public ISelfBuddy CurrentUser
        {
            get { return self; }
        }

        public IEnumerable<IBuddy> Buddies
        {
            get { return Enumerable.Empty<Buddy>(); }
        }

        public bool LoggedIn
        {
            get { return false; }
        }

        public IVoiceChatHandler ActiveVoiceChat
        {
            get { return null; }
        }

        public IChat StartChat(IBuddy buddy)
        {
            throw new NotImplementedException();
        }

        public void Login(string username, IBuddyProperties properties)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        #endregion

        class DummySelfBuddy: Buddy, ISelfBuddy
        {
            public new string DisplayName { get; set; }
            public new UserStatus Status { get; set; }

            public DummySelfBuddy():base(String.Empty, String.Empty, UserStatus.Offline, null, new BuddyProperties())
            {

            }
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
