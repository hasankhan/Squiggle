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

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyOnline = delegate { };
        public event EventHandler<BuddyEventArgs> BuddyOffline = delegate { };

        public List<Buddy> Buddies { get; set; }
        public Buddy CurrentUser { get; set; }

        public ChatClient(IPEndPoint localEndPoint, short presencePort, int keepAliveTime)
        {
            chatService = new ChatService();
            Buddies = new List<Buddy>();
            chatService.ResolveEndPoint += new EventHandler<ResolveEndPointEventArgs>(chatService_ResolveEndPoint);
            chatService.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatService_ChatStarted);
            presenceService = new PresenceService(localEndPoint, presencePort, keepAliveTime);
            presenceService.UserOffline += new EventHandler<UserEventArgs>(presenceService_UserOffline);
            presenceService.UserOnline += new EventHandler<UserEventArgs>(presenceService_UserOnline);
            this.localEndPoint = localEndPoint;
        }

        void chatService_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            ChatStarted(this, e);
        }

        void presenceService_UserOnline(object sender, UserEventArgs e)
        {
            SetUserStatus(e.User, UserStatus.Online);
        }

        void presenceService_UserOffline(object sender, UserEventArgs e)
        {
            SetUserStatus(e.User, UserStatus.Offline);            
        }

        void SetUserStatus(UserInfo user, UserStatus status)
        {
            lock (this)
            {
                var buddy = GetBuddyByAddress(user.ChatEndPoint);
                if (buddy == null)
                {
                    buddy = new Buddy(this) { DisplayName = user.UserFriendlyName, DisplayMessage = String.Empty, EndPoint = user.ChatEndPoint};
                    Buddies.Add(buddy);
                }
                buddy.Status = status;
                OnBuddyStatusChanged(buddy);
            }
        }

        void OnBuddyStatusChanged(Buddy buddy)
        {
            var args = new BuddyEventArgs() { Buddy = buddy };
            if (buddy.Status == UserStatus.Online)
                BuddyOnline(this, args);
            else if (buddy.Status == UserStatus.Offline)
                BuddyOffline(this, args);
        }

        private Buddy GetBuddyByAddress(IPEndPoint endPoint)
        {
            return Buddies.FirstOrDefault(b => b.EndPoint == endPoint);
        }

        void chatService_ResolveEndPoint(object sender, ResolveEndPointEventArgs e)
        {
            var user = presenceService.Users.FirstOrDefault(u => u.ChatEndPoint.ToString() == e.User);
            if (user != null)
                e.EndPoint = user.ChatEndPoint;
        }

        public IChatSession StartChat(IPEndPoint endpoint)
        {
            IChatSession chatSession = chatService.CreateSession(endpoint);
            return chatSession;
        }

        public void Login(string username)
        {
            presenceService.Login(username);
            chatService.Username = username;
            chatService.Start(localEndPoint);

            CurrentUser = new Buddy() 
            { 
                DisplayName = username, 
                DisplayMessage="No display message",
                EndPoint = localEndPoint, 
                Status = UserStatus.Online 
            };
        }

        public void Logout()
        {
            chatService.Stop();
            presenceService.Logout();
        }

    }
}
