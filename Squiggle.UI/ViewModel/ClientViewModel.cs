using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using Squiggle.Chat;
using System.ComponentModel;

namespace Squiggle.UI.ViewModel
{
    public class ClientViewModel: INotifyPropertyChanged
    {
        public event EventHandler ContactListUpdated = delegate { };

        IChatClient chatClient;
        Dispatcher currentDispatcher;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Buddy LoggedInUser { get; set; }
        public ObservableCollection<Buddy> Buddies { get; private set; }

        public bool IsLoggedIn
        {
            get { return chatClient.LoggedIn; }
        }

        public bool AnyoneOnline
        {
            get { return Buddies.Any(b => b.IsOnline); }
        }

        string updateLink;
        public string UpdateLink
        {
            get { return updateLink; }
            set
            {
                updateLink = value;
                OnPropertyChanged("UpdateLink");
            }
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
            Buddies.CollectionChanged += (sender, e)=>OnPropertyChanged("AnyoneOnline");
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

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
