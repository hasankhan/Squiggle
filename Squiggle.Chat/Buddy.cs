using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence;
using System.ComponentModel;
using System.Net;

namespace Squiggle.Chat
{
    public class Buddy: INotifyPropertyChanged, IDisposable
    {
        string displayName;
        UserStatus status;
        string displayMessage;
        Dictionary<string, string> properties;

        protected IChatClient ChatClient { get; private set; }

        public object ID { get; private set; }        

        public virtual string DisplayName
        {
            get { return displayName; }
            set
            {
                displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public virtual string DisplayMessage 
        {
            get { return displayMessage; }
            set
            {
                displayMessage = value;
                OnPropertyChanged("DisplayMessage");
            }
        }

        public virtual UserStatus Status 
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Properties 
        { 
            get { return properties; }
        }

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler Updated = delegate { };

        public Buddy(IChatClient chatClient, object id) : this(chatClient, id, null) { }

        public Buddy(IChatClient chatClient, object id, Dictionary<string, string> properties)
        {
            this.ID = id;
            this.ChatClient = chatClient;
            this.ChatClient.BuddyOffline += new EventHandler<BuddyEventArgs>(chatClient_BuddyOffline);
            this.ChatClient.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            this.ChatClient.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            this.ChatClient.BuddyUpdated += new EventHandler<BuddyEventArgs>(chatClient_BuddyUpdated);

            this.properties = properties ?? new Dictionary<string, string>();
        }

        public virtual void SetProperties(Dictionary<string, string> properties)
        {
            this.properties = properties;
            OnPropertyChanged("Properties");
        }

        public virtual void SetProperty(string key, string value)
        {
            properties[key] = value;
            OnPropertyChanged("Properties");
        }
        
        public IChat StartChat()
        {
            return ChatClient.StartChat(this);
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            if (e.Buddy.Equals(this))
                OnChatStarted(e);
        }

        protected void OnChatStarted(ChatStartedEventArgs e)
        {
            ChatStarted(this, e);
        }

        void chatClient_BuddyOnline(object sender, BuddyEventArgs e)
        {
            if (e.Buddy.Equals(this))
                Status = e.Buddy.Status;
        }

        void chatClient_BuddyOffline(object sender, BuddyEventArgs e)
        {
            if (e.Buddy.Equals(this))
                Status = e.Buddy.Status;
        }

        void chatClient_BuddyUpdated(object sender, BuddyEventArgs e)
        {
            if (e.Buddy.Equals(this))
                OnBuddyUpdated();
        }

        protected void OnBuddyUpdated()
        {
            Updated(this, EventArgs.Empty);
        }        

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Buddy)
            {
                var otherBuddy = (Buddy)obj;
                bool equals = this.ID.Equals(otherBuddy.ID);
                return equals;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.ChatClient.BuddyOffline -= new EventHandler<BuddyEventArgs>(chatClient_BuddyOffline);
            this.ChatClient.BuddyOnline -= new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            this.ChatClient.ChatStarted -= new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            this.ChatClient.BuddyUpdated -= new EventHandler<BuddyEventArgs>(chatClient_BuddyUpdated);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
