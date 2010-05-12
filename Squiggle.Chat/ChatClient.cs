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
    public class ChatClient: IChatClient
    {
        IChatService chatService;
        IPresenceService presenceService;
        IPEndPoint localEndPoint;

        public event EventHandler<ChatStartedEventArgs> ChatStarted;
        public List<Buddy> Buddies { get; private set; }
        public Buddy CurrentUser { get; private set; }

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

        private void SetUserStatus(UserInfo user, UserStatus status)
        {
            var buddy = GetBuddyByAddress(user.Address.ToString());
            if (buddy == null)
            {
                buddy = new Buddy() { DisplayName = user.UserFriendlyName, Address = user.Address.ToString()};
                Buddies.Add(buddy);
            }

            buddy.Status = status;
        }

        private Buddy GetBuddyByAddress(string address)
        {
            return Buddies.FirstOrDefault(b => b.Address == address);
        }

        void chatService_ResolveEndPoint(object sender, ResolveEndPointEventArgs e)
        {
            var user = presenceService.Users.FirstOrDefault(u => u.Address.ToString() == e.User);
            if (user != null)
                e.EndPoint = user.ChatEndPoint;
        }

        public IChatSession StartChat(string address)
        {
            IChatSession chatSession = chatService.CreateSession(localEndPoint);
            return chatSession;
        }

        public void Login(string username)
        {
            presenceService.Login(username);
            chatService.Start(localEndPoint);

            CurrentUser = new Buddy() 
            { 
                DisplayName = username, 
                DisplayMessage="No display message",
                Address = localEndPoint.Address.ToString(), 
                Status = UserStatus.Online };
        }

        public void Logout()
        {
            chatService.Stop();
            presenceService.Logout();
        }

    }
}
