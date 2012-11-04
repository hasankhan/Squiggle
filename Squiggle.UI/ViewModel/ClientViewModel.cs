using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Squiggle.Utilities.Threading;
using Squiggle.Client;

namespace Squiggle.UI.ViewModel
{
    public class ClientViewModel: ViewModelBase
    {
        public event EventHandler ContactListUpdated = delegate { };

        IChatClient chatClient;
        Dispatcher dispatcher;

        public ISelfBuddy LoggedInUser { get; set; }
        public ObservableCollection<IBuddy> Buddies { get; private set; }

        public string Title
        {
            get
            {
                if (IsLoggedIn)
                    return String.Format("Squiggle Messenger - {0}", LoggedInUser.DisplayName);
                return "Squiggle Messenger";
            }
        }

        public bool IsLoggedIn
        {
            get { return chatClient.IsLoggedIn; }
        }

        public bool AnyoneOnline
        {
            get { return Buddies.Any(b => b.IsOnline()); }
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

            dispatcher = Dispatcher.CurrentDispatcher;
            LoggedInUser = chatClient.CurrentUser;

            chatClient.BuddyOnline += chatClient_BuddyOnline;
            chatClient.BuddyOffline += chatClient_BuddyOffline;
            chatClient.BuddyUpdated += chatClient_BuddyUpdated;
            chatClient.LoggedIn += chatClient_LoggedInOut;
            chatClient.LoggedOut += chatClient_LoggedInOut;

            Buddies = new ObservableCollection<IBuddy>(chatClient.Buddies);
            Buddies.CollectionChanged += (sender, e) => OnContactListUpdated();
        }

        void chatClient_LoggedInOut(object sender, EventArgs e)
        {
            OnPropertyChanged("IsLoggedIn", "Title");
        }

        void chatClient_BuddyOffline(object sender, BuddyEventArgs e)
        {
            OnContactListUpdated();
        }

        void chatClient_BuddyUpdated(object sender, BuddyEventArgs e)
        {
            OnContactListUpdated();
        }

        void chatClient_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            if (Buddies.Contains(e.Buddy))
                OnContactListUpdated();
            else
                dispatcher.Invoke(()=> Buddies.Add(e.Buddy));
        }

        void OnContactListUpdated()
        {
            ContactListUpdated(this, EventArgs.Empty);
            OnPropertyChanged("AnyoneOnline");
        }
    }
}
