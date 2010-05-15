using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence;
using System.ComponentModel;
using System.Net;

namespace Squiggle.Chat
{
    public class Buddy : INotifyPropertyChanged
    {
        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        IChatClient chatClient;

        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public IPEndPoint EndPoint { get; internal set; }
        public UserStatus Status { get; set; }

        public Buddy() { }
        public Buddy(IChatClient chatClient)
        {
            this.chatClient = chatClient;
            this.chatClient.BuddyOffline += new EventHandler<BuddyEventArgs>(chatClient_BuddyOffline);
            this.chatClient.BuddyOnline += new EventHandler<BuddyEventArgs>(chatClient_BuddyOnline);
            this.chatClient.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Buddy)
            {
                var otherBuddy = (Buddy)obj;
                bool equals = DisplayName.Equals(otherBuddy.DisplayName);
                if (equals)
                    equals = DisplayMessage.Equals(otherBuddy.DisplayMessage);
                //if (equals)
                //    equals = Address.Equals(otherBuddy.Address);
                if (equals)
                    equals = Status.Equals(otherBuddy.Status);

                return equals;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public IChatSession StartChat()
        {
            return chatClient.StartChat(EndPoint);
        }

        public void EndChat()
        {
            chatClient.EndChat(EndPoint);
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            ChatStarted(this, e);
        }

        void chatClient_BuddyOnline(object sender, BuddyEventArgs e)
        {
            Status = UserStatus.Online;
        }

        void chatClient_BuddyOffline(object sender, BuddyEventArgs e)
        {
            Status = UserStatus.Offline;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
