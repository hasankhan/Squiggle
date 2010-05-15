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
            presenceService.Login(username);

            CurrentUser = new Buddy(this, localEndPoint) 
            { 
                DisplayName = username, 
                DisplayMessage="No display message",
                Status = UserStatus.Online 
            };
        }

        public void Logout()
        {
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
            lock (buddies)
            {
                var buddy = GetBuddyByAddress(user.ChatEndPoint);
                if (buddy == null)
                {
                    buddy = new Buddy(this, user.ChatEndPoint) { DisplayName = user.UserFriendlyName, 
                                                                 DisplayMessage = String.Empty, 
                                                                };
                    buddies.Add(buddy);
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
    }
}
