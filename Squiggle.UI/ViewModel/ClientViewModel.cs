using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Squiggle.Client;

namespace Squiggle.UI.ViewModel
{
    public class ClientViewModel: ViewModelBase
    {
        public event EventHandler ContactListUpdated = delegate { };

        IChatClient chatClient;
        Dispatcher currentDispatcher;

        public ISelfBuddy LoggedInUser { get; set; }
        public ObservableCollection<IBuddy> Buddies { get; private set; }

        public bool IsLoggedIn
        {
            get { return chatClient.IsLoggedIn; }
        }

        public bool AnyoneOnline
        {
            get { return Buddies.Any(b => !b.IsOnline()); }
        }

        string updateLink;
        public string UpdateLink
        {
            get { return updateLink; }
            set { Set(()=>UpdateLink, ref updateLink, value); }
        }

        ICommand cancelUpdateCommand;
        public ICommand CancelUpdateCommand
        {
            get { return cancelUpdateCommand; }
            set { Set(() => cancelUpdateCommand, ref cancelUpdateCommand, value); }
        }

        public ClientViewModel(IChatClient chatClient)
        {
            this.chatClient = chatClient;

            currentDispatcher = Dispatcher.CurrentDispatcher;
            LoggedInUser = chatClient.CurrentUser;

            chatClient.BuddyOnline += chatClient_BuddyOnline;
            chatClient.BuddyOffline += chatClient_BuddyOffline;
            chatClient.BuddyUpdated += chatClient_BuddyUpdated;
            chatClient.LoggedIn += chatClient_LoggedInOut;
            chatClient.LoggedOut += chatClient_LoggedInOut;

            Buddies = new ObservableCollection<IBuddy>(chatClient.Buddies);
            Buddies.CollectionChanged += (sender, e)=>OnPropertyChanged("AnyoneOnline");
        }

        void chatClient_LoggedInOut(object sender, EventArgs e)
        {
            OnPropertyChanged("IsLoggedIn");
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
