using System;
using System.Collections.Generic;
using System.Net;
using Squiggle.Chat.Services.Presence.Transport;

namespace Squiggle.Chat.Services.Presence
{
    class PresenceService : IPresenceService
    {
        UserDiscovery discovery;
        PresenceChannel channel;
        KeepAliveService keepAlive;

        UserInfo thisUser;

        public event EventHandler<UserOnlineEventArgs> UserOnline = delegate { };
        public event EventHandler<UserEventArgs> UserOffline = delegate { };
        public event EventHandler<UserEventArgs> UserUpdated = delegate { };

        public IEnumerable<UserInfo> Users
        {
            get { return discovery.Users; }
        }

        public PresenceService(IPEndPoint chatEndPoint, IPEndPoint presenceEndPoint, IPEndPoint presenceServiceEndPoint, TimeSpan keepAliveTime)
        {
            thisUser = new UserInfo()
            {
                ChatEndPoint = chatEndPoint,
                KeepAliveSyncTime = keepAliveTime,
                PresenceEndPoint = presenceServiceEndPoint
            };

            channel = new PresenceChannel(presenceEndPoint, presenceServiceEndPoint);
            channel.UserInfo = thisUser;

            this.discovery = new UserDiscovery(channel);
            discovery.UserOnline += new EventHandler<UserEventArgs>(discovery_UserOnline);
            discovery.UserOffline += new EventHandler<UserEventArgs>(discovery_UserOffline);
            discovery.UserUpdated += new EventHandler<UserEventArgs>(discovery_UserUpdated);
            discovery.UserDiscovered += new EventHandler<UserEventArgs>(discovery_UserDiscovered);

            this.keepAlive = new KeepAliveService(channel, thisUser, keepAliveTime);
            this.keepAlive.UserLost += new EventHandler<UserEventArgs>(keepAlive_UserLost);
            this.keepAlive.UserReturned += new EventHandler<UserEventArgs>(keepAlive_UserReturned);
        }             

        public void Login(string name, string displayMessage, Dictionary<string, string> properties)
        {
            thisUser.UserFriendlyName = name;
            thisUser.DisplayMessage = displayMessage;
            thisUser.Status = UserStatus.Online;
            thisUser.Properties = properties;

            channel.Start();
            discovery.Login(thisUser);
            keepAlive.Start();
        }

        public void Update(string name, string displayMessage, Dictionary<string, string> properties, UserStatus status)
        {
            thisUser.UserFriendlyName = name;
            thisUser.DisplayMessage = displayMessage;
            thisUser.Status = status;
            thisUser.Properties = properties;
            discovery.Update(thisUser);
        }

        public void Logout()
        {
            discovery.Logout();
            keepAlive.Stop();
            channel.Stop();
        }

        void discovery_UserUpdated(object sender, UserEventArgs e)
        {
            keepAlive.HeIsAlive(e.User);
            UserUpdated(this, e);
        }

        void keepAlive_UserReturned(object sender, UserEventArgs e)
        {
            OnUserOnline(e, false);
        }

        void keepAlive_UserLost(object sender, UserEventArgs e)
        {
            OnUserOffline(e);
        }        

        void discovery_UserOnline(object sender, UserEventArgs e)
        {
            OnUserOnline(e, false);
        }

        void discovery_UserDiscovered(object sender, UserEventArgs e)
        {
            OnUserOnline(e, true);
        }   

        void discovery_UserOffline(object sender, UserEventArgs e)
        {
            OnUserOffline(e);
        }

        void OnUserOnline(UserEventArgs e, bool discovered)
        {
            keepAlive.MonitorUser(e.User);
            UserOnline(this, new UserOnlineEventArgs() { User = e.User, Discovered = discovered });
        }

        void OnUserOffline(UserEventArgs e)
        {
            keepAlive.LeaveUser(e.User);
            UserOffline(this, e);
        }                   

        #region IDisposable Members

        public void Dispose()
        {
            Logout();
        }

        #endregion
    }    
}
