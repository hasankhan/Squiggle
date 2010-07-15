using System;
using System.Collections.Generic;
using System.Net;
using Squiggle.Chat.Services;
using Squiggle.Chat.Services.Chat;
using Squiggle.Chat.Services.Presence;

namespace Squiggle.Chat
{
    public class ChatClient: IChatClient
    {
        IChatService chatService;
        IPresenceService presenceService;
        IPEndPoint localEndPoint;
        BuddyList buddies;

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler<BuddyOnlineEventArgs> BuddyOnline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyOffline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyUpdated = delegate { };

        public Buddy CurrentUser { get; private set; }

        public IEnumerable<Buddy> Buddies 
        {
            get { return buddies; }
        }

        public bool LoggedIn { get; private set; }

        public ChatClient(IPEndPoint chatEndPoint, IPEndPoint presenceEndPoint, TimeSpan keepAliveTime)
        {
            chatService = new ChatService();
            buddies = new BuddyList();
            chatService.ChatStarted += new EventHandler<Squiggle.Chat.Services.ChatStartedEventArgs>(chatService_ChatStarted);
            presenceService = new PresenceService(chatEndPoint, presenceEndPoint, chatEndPoint, keepAliveTime);
            presenceService.UserOffline += new EventHandler<UserEventArgs>(presenceService_UserOffline);
            presenceService.UserOnline += new EventHandler<UserOnlineEventArgs>(presenceService_UserOnline);
            presenceService.UserUpdated += new EventHandler<UserEventArgs>(presenceService_UserUpdated);
            this.localEndPoint = chatEndPoint;
        }        

        public IChat StartChat(Buddy buddy)
        {
            var endpoint = (IPEndPoint)buddy.ID;
            IChatSession session = chatService.CreateSession(endpoint);
            var chat = new Chat(session, buddy, id=>buddies[id]);
            return chat;
        }        

        public void Login(string username, string displayMessage, Dictionary<string, string> properties)
        {
            chatService.Username = username;
            chatService.Start(localEndPoint);
            presenceService.Login(username, displayMessage, properties);

            var self = new SelfBuddy(this, localEndPoint, properties) 
            { 
                DisplayName = username,
                DisplayMessage = displayMessage,
                Status = UserStatus.Online,
            };
            self.EnableUpdates = true;
            CurrentUser = self;
            LoggedIn = true;
        }        

        public void Logout()
        {
            LoggedIn = false;
            foreach (Buddy buddy in buddies)
                buddy.Dispose();
            buddies.Clear();
            chatService.Stop();
            presenceService.Logout();
        }
        
        void Update()
        {
            var properties = CurrentUser.Properties.ToDictionary();
            presenceService.Update(CurrentUser.DisplayName, CurrentUser.DisplayMessage, properties, CurrentUser.Status);
        }

        void chatService_ChatStarted(object sender, Squiggle.Chat.Services.ChatStartedEventArgs e)
        {
            var buddyList = new List<Buddy>();
            foreach (IPEndPoint user in e.Session.RemoteUsers)
            {
                Buddy buddy = buddies[user];
                if (buddy != null)
                    buddyList.Add(buddy);
            }
            if (buddyList.Count > 0)
            {
                var chat = new Chat(e.Session, buddyList, id=>buddies[id]);
                ChatStarted(this, new ChatStartedEventArgs() { Chat = chat, Buddies = buddyList });
            }
        }

        void presenceService_UserUpdated(object sender, UserEventArgs e)
        {
            var buddy = buddies[e.User.ChatEndPoint];
            if (buddy != null)
            {
                UserStatus lastStatus = buddy.Status;
                buddy.Status = e.User.Status;
                buddy.DisplayMessage = e.User.DisplayMessage;
                buddy.DisplayName = e.User.UserFriendlyName;
                buddy.SetProperties(e.User.Properties);

                if (lastStatus != UserStatus.Offline && buddy.Status == UserStatus.Offline)
                    OnBuddyOffline(buddy);
                else if (lastStatus == UserStatus.Offline && buddy.Status != UserStatus.Offline)
                    OnBuddyOnline(buddy, false);
                else
                    OnBuddyUpdated(buddy);
            }
        }              

        void presenceService_UserOnline(object sender, UserOnlineEventArgs e)
        {
            var buddy = buddies[e.User.ChatEndPoint];
            if (buddy == null)
            {
                buddy = new Buddy(this, e.User.ChatEndPoint, e.User.Properties)
                {
                    DisplayName = e.User.UserFriendlyName,
                    Status = e.User.Status,
                    DisplayMessage = e.User.DisplayMessage
                };
                buddies.Add(buddy);
            }
            else
                buddy.Status = e.User.Status;
            OnBuddyOnline(buddy, e.Discovered);
        }        

        void presenceService_UserOffline(object sender, UserEventArgs e)
        {
            var buddy = buddies[e.User.ChatEndPoint];
            if (buddy != null)
            {
                buddy.Status = UserStatus.Offline;
                OnBuddyOffline(buddy);
            }
        }

        void OnBuddyUpdated(Buddy buddy)
        {
            BuddyUpdated(this, new BuddyEventArgs( buddy ));
        } 

        void OnBuddyOnline(Buddy buddy, bool discovered)
        {
            BuddyOnline(this, new BuddyOnlineEventArgs() { Buddy = buddy, Discovered = discovered });
        }

        void OnBuddyOffline(Buddy buddy)
        {
            BuddyOffline(this, new BuddyEventArgs( buddy ));
        }        

        #region IDisposable Members

        public void Dispose()
        {
            Logout();
        }

        #endregion

        class SelfBuddy : Buddy
        {
            public bool EnableUpdates { get; set; }

            public SelfBuddy(IChatClient client, IPEndPoint id, Dictionary<string, string> properties) : base(client, id, properties) { }

            public override string DisplayMessage
            {
                get
                {
                    return base.DisplayMessage;
                }
                set
                {
                    base.DisplayMessage = value;
                    Update();
                }
            }
            
            public override string DisplayName
            {
                get
                {
                    return base.DisplayName;
                }
                set
                {
                    base.DisplayName = value;
                    Update();
                }
            }
            
            public override UserStatus Status
            {
                get
                {
                    return base.Status;
                }
                set
                {
                    base.Status = value;
                    Update();
                }
            }

            protected override void OnBuddyPropertiesChanged()
            {
                base.OnBuddyPropertiesChanged();
                Update();
            }

            void Update()
            {
                OnBuddyUpdated();
                if (EnableUpdates)
                    ((ChatClient)base.ChatClient).Update();
            }
        }
    }
}
