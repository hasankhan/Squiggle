using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Net;
using Squiggle.Chat;
using Squiggle.Chat.Services.Presence.Transport;

namespace Squiggle.Chat.Services.Presence
{
    class PresenceService : IPresenceService
    {
        UserDiscovery discovery;
        PresenceChannel channel;
        KeepAliveService keepAlive;

        UserInfo thisUser;

        public event EventHandler<UserEventArgs> UserOnline;
        public event EventHandler<UserEventArgs> UserOffline;

        public IEnumerable<UserInfo> Users
        {
            get { return discovery.Users; }
        }

        public PresenceService(IPEndPoint chatEndPoint, int presencePort, int keepAliveTime)
        {
            thisUser = new UserInfo()
            {
                ChatEndPoint = chatEndPoint,
                KeepAliveSyncTime = keepAliveTime
            };

            channel = new PresenceChannel(presencePort);

            this.discovery = new UserDiscovery(channel);
            discovery.UserOnline += new EventHandler<UserEventArgs>(discovery_UserOnline);
            discovery.UserOffline += new EventHandler<UserEventArgs>(discovery_UserOffline);

            this.keepAlive = new KeepAliveService(channel, thisUser, keepAliveTime);
            this.keepAlive.UserLost += new EventHandler<UserEventArgs>(keepAlive_UserLost);
            this.keepAlive.UserReturned += new EventHandler<UserEventArgs>(keepAlive_UserReturned);
        }

        public void Login(string name)
        {
            thisUser.UserFriendlyName = name;
            
            channel.Start();
            discovery.Login(thisUser);
            keepAlive.Start();
        }

        public void Logout()
        {
            channel.Stop();
            discovery.Logout();
            keepAlive.Stop();
        }

        void keepAlive_UserReturned(object sender, UserEventArgs e)
        {
            OnUserOnline(e);
        }

        void keepAlive_UserLost(object sender, UserEventArgs e)
        {
            OnUserOffline(e);
        }        

        void discovery_UserOnline(object sender, UserEventArgs e)
        {
            discovery.SayHi();
            OnUserOnline(e);
        }        

        void discovery_UserOffline(object sender, UserEventArgs e)
        {
            OnUserOffline(e);
        }

        void OnUserOnline(UserEventArgs e)
        {
            keepAlive.MonitorUser(e.User);
            UserOnline(this, e);
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
