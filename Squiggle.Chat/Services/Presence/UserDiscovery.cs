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
        public event EventHandler<UserEventArgs> UserDiscovered = delegate { };

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
            var message = LoginMessage.FromUserInfo(thisUser);
            channel.SendMessage(message);
        }        

        public void Update(UserInfo me)
        {
            thisUser = me;
            var message = UserUpdateMessage.FromUserInfo(thisUser);
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
            {
                OnLoginMessage((LoginMessage)e.Message, false);
                SayHi();
            }
            else if (e.Message is LogoutMessage)
                OnLogoutMessage((LogoutMessage)e.Message);
            else if (e.Message is HiMessage)
                OnLoginMessage(((HiMessage)e.Message).Convert<LoginMessage>(), true);
            else if (e.Message is UserUpdateMessage)
                OnUpdateMessage((UserUpdateMessage)e.Message);
        }

        void SayHi()
        {
            var message = HiMessage.FromUserInfo(thisUser);
            channel.SendMessage(message);
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

        void OnLoginMessage(LoginMessage message, bool discovered)
        {
            UserInfo newUser = message.GetUser();
            if (newUser.Status != UserStatus.Offline)
            {
                if (onlineUsers.Add(newUser))
                {
                    if (discovered)
                        UserDiscovered(this, new UserEventArgs() { User = newUser });
                    else
                        UserOnline(this, new UserEventArgs() { User = newUser });
                }
                else
                    OnUserUpdated(newUser);
            }
            else
                OnUserOffline(newUser.ChatEndPoint);
        }

        void OnUserUpdated(UserInfo newUser)
        {
            var oldUser = onlineUsers.First(u => u.Equals(newUser));
            if (oldUser != null)
            {
                oldUser.Update(newUser);
                UserUpdated(this, new UserEventArgs() { User = oldUser });
            }
        }

        void OnUpdateMessage(UserUpdateMessage message)
        {
            UserInfo newUser = message.GetUser();
            OnUserUpdated(newUser);
        }
    }
}
