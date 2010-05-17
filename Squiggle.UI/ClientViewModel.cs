using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Squiggle.UI
{
    class ClientViewModel
    {
        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        IChatClient chatClient;
        Dispatcher currentDispatcher;

        public ObservableCollection<Buddy> Buddies { get; private set; }

        public Buddy LoggedInUser { get; set; }


        public ClientViewModel(IChatClient chatClient)
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            this.chatClient = chatClient;
            LoggedInUser = chatClient.CurrentUser;
            this.chatClient.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            this.chatClient.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            Buddies = new ObservableCollection<Buddy>(chatClient.Buddies);
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
