using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Squiggle.Chat;

namespace Squiggle.UI
{
    public class ClientViewModel
    {
        public event EventHandler ContactListUpdated = delegate { };

        IChatClient chatClient;
        Dispatcher currentDispatcher;

        public ObservableCollection<Buddy> Buddies { get; private set; }

        public Buddy LoggedInUser { get; set; }

        public bool IsLoggedIn
        {
            get { return chatClient.LoggedIn; }
        }

        public ClientViewModel(IChatClient chatClient)
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            this.chatClient = chatClient;
            LoggedInUser = chatClient.CurrentUser;
            this.chatClient.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            this.chatClient.BuddyOffline += new EventHandler<BuddyEventArgs>(chatClient_BuddyOffline);
            this.chatClient.BuddyUpdated += new EventHandler<BuddyEventArgs>(chatClient_BuddyUpdated);
            Buddies = new ObservableCollection<Buddy>(chatClient.Buddies);
        }

        void chatClient_BuddyOffline(object sender, BuddyEventArgs e)
        {
            ContactListUpdated(this, EventArgs.Empty);
        }

        void chatClient_BuddyUpdated(object sender, BuddyEventArgs e)
        {
            ContactListUpdated(this, EventArgs.Empty);
        }

        void chatClient_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            if (!Buddies.Contains(e.Buddy))
                currentDispatcher.Invoke(new Action(delegate() { Buddies.Add(e.Buddy); }));
            ContactListUpdated(this, EventArgs.Empty);
        }
    }
}
