using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using Squiggle.Core;
using Squiggle.Core.Presence;

namespace Squiggle.Client
{
    public class Buddy: INotifyPropertyChanged, Squiggle.Client.IBuddy
    {
        string displayName;
        UserStatus status;
        IBuddyProperties properties;
        bool initialized;

        public string Id { get; private set; }
        public IPEndPoint ChatEndPoint { get; private set; }
        public DateTime LastUpdated { get; private set; }

        public event EventHandler Offline = delegate { };
        public event EventHandler Online = delegate { };

        public Buddy(string id, string displayName, UserStatus status, IPEndPoint chatEndPoint, IBuddyProperties properties)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.Status = status;
            this.ChatEndPoint = chatEndPoint;

            this.properties = properties;
            this.properties.PropertyChanged += (sender, e) => OnBuddyPropertiesChanged();
            initialized = true;

            LastUpdated = DateTime.Now;
        }

        public virtual string DisplayName
        {
            get { return displayName; }
            protected set
            {
                displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public virtual UserStatus Status
        {
            get { return status; }
            protected internal set
            {
                var lastStatus = status;
                status = value;

                OnPropertyChanged("Status");
                OnPropertyChanged("IsOnline");

                if (lastStatus == UserStatus.Offline && status != UserStatus.Offline)
                    Online(this, EventArgs.Empty);
                else if (lastStatus != UserStatus.Offline && status == UserStatus.Offline)
                    Offline(this, EventArgs.Empty);
            }
        }

        public bool IsOnline
        {
            get { return Status != UserStatus.Offline; }
        }
        
        public IBuddyProperties Properties
        {
            get { return properties; }
        }

        protected virtual void OnBuddyPropertiesChanged()
        {
            OnPropertyChanged("Properties");
        }

        internal void Update(UserStatus status, string displayName, IPEndPoint chatEndPoint, IDictionary<string, string> properties)
        {
            this.Status = status;
            this.DisplayName = displayName;
            this.ChatEndPoint = chatEndPoint;
            this.properties = new BuddyProperties(properties ?? new Dictionary<string, string>());
            this.properties.PropertyChanged += (sender, e) => OnBuddyPropertiesChanged();
            OnBuddyPropertiesChanged();
            OnPropertyChanged("ChatEndPoint");
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Buddy)
            {
                var otherBuddy = (Buddy)obj;
                bool equals = this.Id.Equals(otherBuddy.Id);
                return equals;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

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
