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
        public event EventHandler<UserEventArgs> UserUpdated = delegate { };

        public UserDiscovery(PresenceChannel channel)
        {
            this.channel = channel;
            this.onlineUsers = new HashSet<UserInfo>();
        }
        
        public void Login(UserInfo me)
        {
            thisUser = me;

            channel.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);
            channel.MessageReceived += new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);

            SayHi();
        }

        public void SayHi()
        {
            var message = LoginMessage.FromUserInfo(thisUser);
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
            IPEndPoint chatEndPoint = message.ChatEndPoint;
            OnUserOffline(chatEndPoint);
        }

        void OnUserOffline(IPEndPoint chatEndPoint)
        {
            var user = onlineUsers.FirstOrDefault(u => u.ChatEndPoint.Equals(chatEndPoint));
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
                UserInfo newUser = message.GetUser();
                if (newUser.Status != UserStatus.Offline)
                {
                    if (onlineUsers.Add(newUser))
                        UserOnline(this, new UserEventArgs() { User = newUser });
                    else
                    {
                        var oldUser = onlineUsers.First(u=>u.Equals(newUser));
                        oldUser.Update(newUser);
                        UserUpdated(this, new UserEventArgs() { User = oldUser });
                    }
                }
                else
                    OnUserOffline(newUser.ChatEndPoint);
            }
        }        
    }
}
