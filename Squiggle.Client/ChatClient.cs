using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Squiggle.Core;
using Squiggle.Core.Chat;
using Squiggle.Core.Presence;
using Squiggle.History;
using Squiggle.Utilities;

namespace Squiggle.Client
{
    public class ChatClient: IChatClient
    {
        IChatService chatService;
        IPresenceService presenceService;
        SquiggleEndPoint chatEndPoint;
        BuddyList buddies;

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler<BuddyOnlineEventArgs> BuddyOnline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyOffline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyUpdated = delegate { };
        public event EventHandler LoggedIn = delegate { };
        public event EventHandler LoggedOut = delegate { };

        public ISelfBuddy CurrentUser { get; private set; }

        public IEnumerable<IBuddy> Buddies 
        {
            get { return buddies; }
        }

        bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                if (_isLoggedIn)
                    LoggedIn(this, EventArgs.Empty);
                else
                    LoggedOut(this, EventArgs.Empty);
            }
        }

        public bool EnableLogging { get; set; }

        public ChatClient()
        {
            buddies = new BuddyList();
            CurrentUser = new SelfBuddy(this, String.Empty, String.Empty, UserStatus.Offline, new BuddyProperties());
        }        

        public IChat StartChat(IBuddy buddy)
        {
            if (!IsLoggedIn)
                throw new InvalidOperationException("Not logged in.");

            IChatSession session = chatService.CreateSession(new SquiggleEndPoint(buddy.Id, ((Buddy)buddy).ChatEndPoint));
            var chat = new Chat(session, CurrentUser, buddy, id=>buddies[id]);
            return chat;
        }        

        public void Login(ChatClientOptions options)
        {
            string username = options.Username.Trim();

            this.chatEndPoint = (SquiggleEndPoint)options.ChatEndPoint;
            StartChatService();

            var presenceOptions = new PresenceServiceOptions()
            {
                ChatEndPoint = (SquiggleEndPoint)options.ChatEndPoint,
                MulticastEndPoint = options.MulticastEndPoint,
                MulticastReceiveEndPoint = options.MulticastReceiveEndPoint,
                PresenceServiceEndPoint = options.PresenceServiceEndPoint,
                KeepAliveTime = options.KeepAliveTime
            };
            StartPresenceService(username, options.UserProperties, presenceOptions);

            var self = (SelfBuddy)CurrentUser;
            self.Update(UserStatus.Online, options.Username, chatEndPoint.Address, options.UserProperties.ToDictionary());
            self.EnableUpdates = true;
            LogStatus(CurrentUser);

            IsLoggedIn = true;
        }        

        public void Logout()
        {
            IsLoggedIn = false;
            chatService.Stop();
            presenceService.Logout();

            var self = (SelfBuddy)CurrentUser;
            self.EnableUpdates = false;
            self.Status = UserStatus.Offline;

            LogStatus(CurrentUser);
        }
        
        void Update()
        {
            LogStatus(CurrentUser);
            var properties = CurrentUser.Properties.Clone();
            presenceService.Update(CurrentUser.DisplayName, properties, CurrentUser.Status);
        }

        void chatService_ChatStarted(object sender, Squiggle.Core.Chat.ChatStartedEventArgs e)
        {
            var buddyList = new List<IBuddy>();
            foreach (SquiggleEndPoint user in e.Session.RemoteUsers)
            {
                Buddy buddy = buddies[user.ClientID];
                if (buddy != null)
                    buddyList.Add(buddy);
            }
            if (buddyList.Count > 0)
            {
                var chat = new Chat(e.Session, CurrentUser, buddyList, id=>buddies[id]);
                ChatStarted(this, new ChatStartedEventArgs() { Chat = chat, Buddies = buddyList });
            }
        }

        void presenceService_UserUpdated(object sender, UserEventArgs e)
        {
            var buddy = buddies[e.User.ID];
            if (buddy != null)
            {
                UserStatus lastStatus = buddy.Status;
                UpdateBuddy(buddy, e.User);

                if (lastStatus != UserStatus.Offline && !buddy.IsOnline)
                    OnBuddyOffline(buddy);
                else if (lastStatus == UserStatus.Offline && buddy.IsOnline)
                    OnBuddyOnline(buddy, false);
                else
                    OnBuddyUpdated(buddy);
            }
        }        

        void presenceService_UserOnline(object sender, UserOnlineEventArgs e)
        {
            var buddy = buddies[e.User.ID];
            if (buddy == null)
            {
                buddy = new Buddy(e.User.ID, e.User.DisplayName, e.User.Status, e.User.ChatEndPoint, new BuddyProperties(e.User.Properties));
                buddies.Add(buddy);
            }
            else
                UpdateBuddy(buddy, e.User);
            
            OnBuddyOnline(buddy, e.Discovered);
        }        

        void presenceService_UserOffline(object sender, UserEventArgs e)
        {
            var buddy = buddies[e.User.ID];
            if (buddy != null)
            {
                buddy.Update(e.User.Status, e.User.DisplayName, e.User.ChatEndPoint, e.User.Properties);
                OnBuddyOffline(buddy);
            }
        }

        void OnBuddyUpdated(Buddy buddy)
        {
            LogStatus(buddy);
            BuddyUpdated(this, new BuddyEventArgs( buddy ));
        } 

        void OnBuddyOnline(IBuddy buddy, bool discovered)
        {
            if (!discovered)
                LogStatus(buddy);
            BuddyOnline(this, new BuddyOnlineEventArgs() { Buddy = buddy, Discovered = discovered });
        }

        void OnBuddyOffline(IBuddy buddy)
        {
            LogStatus(buddy);
            BuddyOffline(this, new BuddyEventArgs( buddy ));
        }

        void UpdateBuddy(IBuddy buddy, IUserInfo user)
        {
            ((Buddy)buddy).Update(user.Status, user.DisplayName, user.ChatEndPoint, user.Properties);
        }

        void LogStatus(IBuddy buddy)
        {
            if (EnableLogging)
                ExceptionMonster.EatTheException(() =>
                {
                    var manager = new HistoryManager();
                    manager.AddStatusUpdate(DateTime.Now, new Guid(buddy.Id), buddy.DisplayName, (int)buddy.Status);
                }, "logging history.");
        }

        void StartPresenceService(string username, IBuddyProperties properties, PresenceServiceOptions presenceOptions)
        {
            presenceService = new PresenceService(presenceOptions);
            presenceService.UserOffline += presenceService_UserOffline;
            presenceService.UserOnline += presenceService_UserOnline;
            presenceService.UserUpdated += presenceService_UserUpdated;
            presenceService.Login(username, properties);
        }

        void StartChatService()
        {
            chatService = new ChatService(chatEndPoint);
            chatService.ChatStarted += chatService_ChatStarted;
            chatService.Start();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Logout();
        }

        #endregion

        class SelfBuddy : Buddy, ISelfBuddy
        {
            IChatClient client;

            public bool EnableUpdates { get; set; }

            public SelfBuddy(IChatClient client, string id, string displayName, UserStatus status, IBuddyProperties properties) : base(id, displayName, status, null, properties)
            {
                this.client = client;
            }

            public new string DisplayName
            {
                get { return base.DisplayName; }
                set
                {
                    base.DisplayName = value;
                    Update();
                }
            }
            
            public new UserStatus Status
            {
                get { return base.Status; }
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
                if (EnableUpdates)
                    ((ChatClient)client).Update();
            }
        }
    }
}
