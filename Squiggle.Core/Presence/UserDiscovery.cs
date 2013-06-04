using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Squiggle.Core.Chat;
using Squiggle.Core.Presence.Transport;
using Squiggle.Core.Presence.Transport.Messages;
using Squiggle.Utilities;

namespace Squiggle.Core.Presence
{    
    class UserDiscovery
    {
        IUserInfo thisUser;
        PresenceChannel channel;
        ConcurrentDictionary<string, IUserInfo> onlineUsers;

        public IDictionary<string, IUserInfo> Users
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
            this.onlineUsers = new ConcurrentDictionary<string, IUserInfo>();
        }
        
        public void Login(IUserInfo me)
        {
            thisUser = me;

            UnsubscribeChannel();

            channel.MessageReceived += channel_MessageReceived;

            var message = Message.FromSender<LoginMessage>(me);
            channel.MulticastMessage(message);
        }        

        public void Update(IUserInfo me)
        {
            thisUser = me;
            var message = Message.FromSender<UserUpdateMessage>(me);
            channel.MulticastMessage(message);
        }

        public void FakeLogout(IUserInfo me)
        {
            thisUser = me;
            var message = Message.FromSender<LogoutMessage>(me);
            channel.MulticastMessage(message);
        }

        public void Logout()
        {
            UnsubscribeChannel();

            var message = Message.FromSender<LogoutMessage>(thisUser);
            channel.MulticastMessage(message);
        }        

        /// <summary>
        /// To tell discovery service that user is lost via keep alive service
        /// </summary>
        /// <param name="id"></param>
        public void UserIsOffline(string id)
        {
            OnUserOffline(id);
        }

        /// <summary>
        /// Asks user to return his presence information. Can be used to get latest update about user presence or to inquire details about discovered users.
        /// </summary>
        /// <param name="user">The presence endpoint of user.</param>
        /// <param name="discovered">If True, UserUpdated event is fired on completion, otherwise UserDiscovered.</param>
        public void UpdateUser(ISquiggleEndPoint user, bool discovered)
        {
            AskForUserInfo(user, discovered ? UserInfoState.PresenceDiscovered : UserInfoState.Update);
        }

        void channel_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ExceptionMonster.EatTheException(() =>
            {
                if (e.Message is LoginMessage)
                    OnLoginMessage(e.Message);
                else if (e.Message is HiMessage)
                    OnHiMessage((HiMessage)e.Message);
                else if (e.Message is HelloMessage)
                    OnHelloMessage((HelloMessage)e.Message);
                else if (e.Message is UserUpdateMessage)
                    OnUpdateMessage((UserUpdateMessage)e.Message);
                else if (e.Message is GiveUserInfoMessage)
                    OnGiveUserInfoMessage((GiveUserInfoMessage)e.Message);
                else if (e.Message is UserInfoMessage)
                    OnUserInfoMessage((UserInfoMessage)e.Message);
                else if (e.Message is LogoutMessage)
                    OnLogoutMessage((LogoutMessage)e.Message);
                    
            }, "handling presence message in userdiscovery class");
        }       

        void OnLogoutMessage(LogoutMessage message)
        {
            OnUserOffline(message.Sender.ClientID);
        }      

        void OnLoginMessage(Message message)
        {
            var reply = PresenceMessage.FromUserInfo<HiMessage>(thisUser);
            reply.Recipient = message.Sender;
            channel.SendMessage(reply); 
        }

        void OnHiMessage(HiMessage message)
        {
            var reply = PresenceMessage.FromUserInfo<HelloMessage>(thisUser);
            reply.Recipient = message.Sender;
            channel.SendMessage(reply);

            UserInfo user = message.GetUser();
            OnPresenceMessage(user, discovered: true);
        }

        void OnHelloMessage(HelloMessage message)
        {
            OnPresenceMessage(message.GetUser(), discovered: false);
        }

        void OnUpdateMessage(UserUpdateMessage message)
        {
            AskForUserInfo(message.Sender, UserInfoState.Update);
        }

        void OnUserInfoMessage(UserInfoMessage message)
        {
            var state = (UserInfoState)message.State;
            if (state == UserInfoState.Update)
                OnUserUpdated(message.GetUser());
            else if (state == UserInfoState.PresenceDiscovered)
                OnPresenceMessage(message.GetUser(), discovered: true);
        }

        void OnGiveUserInfoMessage(GiveUserInfoMessage message)
        {
            var reply = PresenceMessage.FromUserInfo<UserInfoMessage>(thisUser);
            reply.State = message.State;
            reply.Recipient = message.Sender;
            channel.SendMessage(reply);
        }        

        void OnPresenceMessage(IUserInfo user, bool discovered)
        {
            if (user.Status == UserStatus.Offline)
                OnUserOffline(user.ID);
            else if (onlineUsers.TryAdd(user.ID, user))
            {
                if (discovered)
                    UserDiscovered(this, new UserEventArgs() { User = user });
                else
                    UserOnline(this, new UserEventArgs() { User = user });
            }
            else
                OnUserUpdated(user);
        }

        void OnUserOffline(string id)
        {
            IUserInfo user;
            if (onlineUsers.TryRemove(id, out user))
            {
                user.Status = UserStatus.Offline;
                UserOffline(this, new UserEventArgs() { User = user });
            }
        }  

        void OnUserUpdated(IUserInfo newUser)
        {
            IUserInfo oldUser = onlineUsers[newUser.ID];
            if (oldUser == null)
                OnPresenceMessage(newUser, discovered: true);
            else
            {
                oldUser.Update(newUser);
                UserUpdated(this, new UserEventArgs() { User = oldUser });
            }
        }

        void AskForUserInfo(ISquiggleEndPoint user, UserInfoState state)
        {
            var reply = Message.FromSender<GiveUserInfoMessage>(thisUser);
            reply.State = (int)state;
            reply.Recipient = new SquiggleEndPoint(user);
            channel.SendMessage(reply);
        }

        void UnsubscribeChannel()
        {
            channel.MessageReceived -= channel_MessageReceived;
        }

        enum UserInfoState
        {
            Update = 1,
            PresenceDiscovered = 2
        }
    }
}
