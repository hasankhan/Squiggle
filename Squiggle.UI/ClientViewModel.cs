using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Squiggle.UI
{
    public class ClientViewModel
    {
        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler BuddyUpdated = delegate { };
        public event EventHandler BuddyOffline = delegate { };


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
            this.chatClient.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            Buddies = new ObservableCollection<Buddy>(chatClient.Buddies);
        }

        void chatClient_BuddyOffline(object sender, BuddyEventArgs e)
        {
            BuddyOffline(this, EventArgs.Empty);
        }

        void chatClient_BuddyUpdated(object sender, BuddyEventArgs e)
        {
            BuddyUpdated(this, EventArgs.Empty);
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            ChatStarted(this, e);
        }

        void chatClient_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            if (!Buddies.Contains(e.Buddy))
                currentDispatcher.Invoke(new Action(delegate() { Buddies.Add(e.Buddy); }));
        }
    }
}
