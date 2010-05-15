using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Squiggle.Chat.Services.Presence.Transport;
using Squiggle.Chat.Services.Presence.Transport.Messages;

namespace Squiggle.Chat.Services.Presence
{    
    class UserDiscovery
    {
        UserInfo thisUser;
        PresenceChannel channel;
        HashSet<UserInfo> onlineUsers;

        public IEnumerable<UserInfo> Users
        {
            get { return onlineUsers; }
        }

        public event EventHandler<UserEventArgs> UserOnline = delegate { };
        public event EventHandler<UserEventArgs> UserOffline = delegate { };        

        public UserDiscovery(PresenceChannel channel)
        {
            this.channel = channel;
            this.onlineUsers = new HashSet<UserInfo>();
        }
        
        public void Login(UserInfo me)
        {
            thisUser = me;
            channel.MessageReceived += new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);

            SayHi();
        }

        public void SayHi()
        {
            var message = new LoginMessage()
            {
                ChatEndPoint = thisUser.ChatEndPoint,
                KeepAliveSyncTime = thisUser.KeepAliveSyncTime,
                UserFriendlyName = thisUser.UserFriendlyName
            };
            channel.SendMessage(message);
        }

        public void Logout()
        {
            channel.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);

            var message = new LogoutMessage() { ChatEndPoint = thisUser.ChatEndPoint };
            channel.SendMessage(message);
        }

        void channel_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is LoginMessage)
                OnLoginMessage((LoginMessage)e.Message);
            else if (e.Message is LogoutMessage)
                OnLogoutMessage((LogoutMessage)e.Message);
        }

        void OnLogoutMessage(LogoutMessage message)
        {
            var user = onlineUsers.FirstOrDefault(u => u.ChatEndPoint.Equals(message.ChatEndPoint));
            if (user != null)
            {
                onlineUsers.Remove(user);
                UserOffline(this, new UserEventArgs() { User = user });
            }
        }

        void OnLoginMessage(LoginMessage message)
        {
            if (!message.ChatEndPoint.Equals(thisUser.ChatEndPoint))
            {
                var user = new UserInfo()
                {
                    ChatEndPoint = message.ChatEndPoint,
                    KeepAliveSyncTime = message.KeepAliveSyncTime,
                    UserFriendlyName = message.UserFriendlyName
                };
                if (onlineUsers.Add(user))
                    UserOnline(this, new UserEventArgs() { User = user });
            }
        }        
    }
}
