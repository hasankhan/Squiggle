using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Diagnostics;

namespace Squiggle.Chat
{
    public class Buddy: INotifyPropertyChanged, IDisposable
    {
        string displayName;
        UserStatus status;
        BuddyProperties properties;
        bool initialized;

        protected IChatClient ChatClient { get; private set; }
        public object ID { get; private set; }
        public IPEndPoint ChatEndPoint { get; private set; }
        public DateTime LastUpdated { get; set; }

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler Offline = delegate { };
        public event EventHandler Online = delegate { };

        public Buddy()
        {
        }

        public Buddy(IChatClient chatClient, object id, IPEndPoint chatEndPoint, BuddyProperties properties)
        {
            this.ID = id;
            this.ChatEndPoint = chatEndPoint;
            this.ChatClient = chatClient;
            this.ChatClient.BuddyOffline += new EventHandler<BuddyEventArgs>(chatClient_BuddyOffline);
            this.ChatClient.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            this.ChatClient.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);

            this.properties = properties;
            this.properties.PropertyChanged += (sender, e) => OnBuddyPropertiesChanged();
            initialized = true;

            LastUpdated = DateTime.Now;
        }

        public virtual string DisplayName
        {
            get { return displayName; }
            set
            {
                displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public virtual UserStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("IsOnline");
            }
        }

        public bool IsOnline
        {
            get { return Status != UserStatus.Offline; }
        }
        
        public BuddyProperties Properties
        {
            get { return properties; }
        }

        protected virtual void OnBuddyPropertiesChanged()
        {
            OnPropertyChanged("Properties");
        }

        public void SetProperties(Dictionary<string, string> properties)
        {
            this.properties = new BuddyProperties(properties ?? new Dictionary<string, string>());
            this.properties.PropertyChanged += (sender, e) => OnBuddyPropertiesChanged();
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
            {
                Status = e.Buddy.Status;
                Online(this, EventArgs.Empty);
            }
        }

        void chatClient_BuddyOffline(object sender, BuddyEventArgs e)
        {
            if (e.Buddy.Equals(this))
            {
                Status = e.Buddy.Status;
                Offline(this, EventArgs.Empty);
            }
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
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        void OnPropertyChanged(string name)
        {
            if (initialized)
            {
                LastUpdated = DateTime.Now;
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
