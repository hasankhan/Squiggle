using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Squiggle.Chat.Services.Presence;
using Squiggle.Chat.Services.Chat;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace Squiggle.Chat
{
    public class BuddyEventArgs : EventArgs
    {
        public Buddy Buddy { get; set; }
    }

    public class ChatClient: IChatClient
    {
        IChatService chatService;
        IPresenceService presenceService;
        IPEndPoint localEndPoint;
        List<Buddy> buddies;

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyOnline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyOffline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyUpdated = delegate { };

        public Buddy CurrentUser { get; private set; }

        public IEnumerable<Buddy> Buddies 
        {
            get { return buddies; }
        }

        public ChatClient(IPEndPoint localEndPoint, short presencePort, TimeSpan keepAliveTime)
        {
            chatService = new ChatService();
            buddies = new List<Buddy>();
            chatService.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatService_ChatStarted);
            presenceService = new PresenceService(localEndPoint, presencePort, keepAliveTime);
            presenceService.UserOffline += new EventHandler<UserEventArgs>(presenceService_UserOffline);
            presenceService.UserOnline += new EventHandler<UserEventArgs>(presenceService_UserOnline);
            presenceService.UserUpdated += new EventHandler<UserEventArgs>(presenceService_UserUpdated);
            this.localEndPoint = localEndPoint;
        }        

        public IChatSession StartChat(Buddy buddy)
        {
            var endpoint = (IPEndPoint)buddy.ID;
            IChatSession chatSession = chatService.CreateSession(endpoint);
            return chatSession;
        }

        public void EndChat(Buddy buddy)
        {
            var endpoint = (IPEndPoint)buddy.ID;
            chatService.RemoveSession(endpoint);
        }

        public void Login(string username)
        {
            chatService.Username = username;
            chatService.Start(localEndPoint);
            presenceService.Login(username, String.Empty);

            CurrentUser = new SelfBuddy(this, localEndPoint) 
            { 
                DisplayName = username, 
                DisplayMessage = String.Empty,
                Status = UserStatus.Online 
            };
        }

        private void Update()
        {
            presenceService.Update(CurrentUser.DisplayName, CurrentUser.DisplayMessage, CurrentUser.Status);
        }

        public void Logout()
        {
            foreach (Buddy buddy in buddies)
                buddy.Dispose();
            buddies.Clear();
            chatService.Stop();
            presenceService.Logout();
        }

        Buddy GetBuddyByAddress(IPEndPoint endPoint)
        {
            Buddy buddy = buddies.FirstOrDefault(b => b.ID.Equals(endPoint));
            return buddy;
        }

        void chatService_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            e.Buddy = GetBuddyByAddress(e.Session.RemoteUser);
            ChatStarted(this, e);
        }

        void presenceService_UserUpdated(object sender, UserEventArgs e)
        {
            var buddy = GetBuddyByAddress(e.User.ChatEndPoint);
            if (buddy != null)
            {
                buddy.Status = e.User.Status;
                buddy.DisplayMessage = e.User.DisplayMessage;
                buddy.DisplayName = e.User.DisplayMessage;
                BuddyUpdated(this, new BuddyEventArgs() { Buddy = buddy });
            }
        }       

        void presenceService_UserOnline(object sender, UserEventArgs e)
        {
            var buddy = GetBuddyByAddress(e.User.ChatEndPoint);
            if (buddy == null)
            {
                buddy = new Buddy(this, e.User)
                {
                    DisplayName = e.User.UserFriendlyName,
                    Status = e.User.Status,
                    DisplayMessage = e.User.DisplayMessage,
                };
                buddies.Add(buddy);
                BuddyOnline(this, new BuddyEventArgs() { Buddy = buddy });
            }
        }

        void presenceService_UserOffline(object sender, UserEventArgs e)
        {
            var buddy = GetBuddyByAddress(e.User.ChatEndPoint);
            if (buddy == null)
                BuddyOffline(this, new BuddyEventArgs(){Buddy = buddy});
        }        

        void OnBuddyStatusChanged(Buddy buddy)
        {
            var args = new BuddyEventArgs() { Buddy = buddy };
            if (buddy.Status == UserStatus.Online)
                BuddyOnline(this, args);
            else if (buddy.Status == UserStatus.Offline)
                BuddyOffline(this, args);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Logout();
        }

        #endregion

        class SelfBuddy : Buddy
        {
            public SelfBuddy(IChatClient client, IPEndPoint id) : base(client, id) { }

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

            void Update()
            {
                ((ChatClient)base.ChatClient).Update();
            }
        }
    }
}
