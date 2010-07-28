using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
            var message = Message.FromUserInfo<LoginMessage>(thisUser);
            channel.SendMessage(message);
        }        

        public void Update(UserInfo me)
        {
            thisUser = me;
            var message = Message.FromUserInfo<UserUpdateMessage>(thisUser);
            channel.SendMessage(message);
        }

        public void Logout()
        {
            channel.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);

            var message = Message.FromUserInfo<LogoutMessage>(thisUser);
            channel.SendMessage(message);
        }

        public void DiscoverUser(IPEndPoint presenceEndPoint)
        {
            UserInfo user = channel.GetUserInfo(presenceEndPoint);
            OnPresenceMessage(user, true);
        }

        void channel_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                if (e.Message is LoginMessage)
                {
                    OnLoginMessage(e.Message);
                    SayHi(e.Message.PresenceEndPoint);
                }
                else if (e.Message is LogoutMessage)
                    OnLogoutMessage((LogoutMessage)e.Message);
                else if (e.Message is HiMessage)
                    OnHiMessage((HiMessage)e.Message);
                else if (e.Message is UserUpdateMessage)
                    OnUpdateMessage((UserUpdateMessage)e.Message);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }            
        }        

        void SayHi(IPEndPoint presenceEndPoint)
        {
            var message = PresenceMessage.FromUserInfo<HiMessage>(thisUser);
            channel.SendMessage(message, presenceEndPoint);            
        }

        void OnLogoutMessage(LogoutMessage message)
        {
            IPEndPoint presenceEndPoint = message.PresenceEndPoint;
            OnUserOffline(presenceEndPoint);
        }

        void OnUserOffline(IPEndPoint endPoint)
        {
            var user = onlineUsers.FirstOrDefault(u => u.PresenceEndPoint.Equals(endPoint));
            if (user != null)
            {
                onlineUsers.Remove(user);
                UserOffline(this, new UserEventArgs() { User = user });
            }
        }        

        void OnLoginMessage(Message message)
        {
            UserInfo newUser = channel.GetUserInfo(message.PresenceEndPoint);
            if (newUser != null)
                OnPresenceMessage(newUser, false);
        }

        void OnPresenceMessage(UserInfo user, bool discovered)
        {
            if (user.Status != UserStatus.Offline)
            {
                if (onlineUsers.Add(user))
                {
                    if (discovered)
                        UserDiscovered(this, new UserEventArgs() { User = user });
                    else
                        UserOnline(this, new UserEventArgs() { User = user });
                }
                else
                    OnUserUpdated(user);
            }
            else
                OnUserOffline(user.PresenceEndPoint);
        }

        void OnUserUpdated(UserInfo newUser)
        {
            var oldUser = onlineUsers.FirstOrDefault(u => u.Equals(newUser));
            if (oldUser != null)
            {
                oldUser.Update(newUser);
                UserUpdated(this, new UserEventArgs() { User = oldUser });
            }
        }

        void OnHiMessage(HiMessage message)
        {
            UserInfo user = message.GetUser();
            OnPresenceMessage(user, true);
        }

        void OnUpdateMessage(UserUpdateMessage message)
        {
            UserInfo newUser = channel.GetUserInfo(message.PresenceEndPoint);
            if (newUser != null)
                OnUserUpdated(newUser);
        }        
    }
}
