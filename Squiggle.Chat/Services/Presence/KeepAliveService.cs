using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Timers;
using System.Net.Sockets;
using Squiggle.Chat.Services.Presence.Transport;

namespace Squiggle.Chat.Services.Presence
{
    class KeepAliveService : IDisposable
    {
        Timer timer;
        PresenceChannel channel;
        int keepAliveSyncTime;
        Message keepAliveMessage;
        Dictionary<UserInfo, DateTime> aliveUsers;
        HashSet<UserInfo> lostUsers;

        public UserInfo User { get; private set; }

        public event EventHandler<UserEventArgs> UserLost = delegate { };
        public event EventHandler<UserEventArgs> UserReturned = delegate { };

        public KeepAliveService(PresenceChannel channel, UserInfo user, int keepAliveSyncTime)
        {
            this.channel = channel;
            this.keepAliveSyncTime = keepAliveSyncTime;
            this.User = user;
            aliveUsers = new Dictionary<UserInfo, DateTime>();
            lostUsers = new HashSet<UserInfo>();
            keepAliveMessage = new KeepAliveMessage() { ChatEndPoint = user.ChatEndPoint };
        }

        public void Start()
        {
            this.timer = new Timer();
            timer.Interval = keepAliveSyncTime * 1000; // seconds
            this.timer.AutoReset = true;
            this.timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            this.timer.Start();
            channel.MessageReceived += new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);
        }

        void channel_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is KeepAliveMessage)
                OnKeepAliveMessage((KeepAliveMessage)e.Message);
        }        

        public void ImAlive()
        {
            channel.SendMessage(keepAliveMessage);
        }

        public void MonitorUser(UserInfo user)
        {
            HeIsAlive(user);
        }

        public void LeaveUser(UserInfo user)
        {
            HeIsGone(user);
            lostUsers.Remove(user);
        }

        public void Stop()
        {
            lostUsers.Clear();
            aliveUsers.Clear();

            timer.Stop();
            timer = null;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ImAlive();

            List<UserInfo> gone = GetLostUsers();

            foreach (UserInfo user in gone)
            {
                lostUsers.Add(user);
                HeIsGone(user);
                UserLost(this, new UserEventArgs() { User = user });
            }
        }        

        void OnKeepAliveMessage(KeepAliveMessage message)
        {
            var user = new UserInfo() { ChatEndPoint = message.ChatEndPoint };
            if (!User.Equals(user))
                HeIsAlive(user);
        }

        List<UserInfo> GetLostUsers()
        {
            lock (aliveUsers)
            {
                var now = DateTime.Now;
                List<UserInfo> gone = new List<UserInfo>();
                foreach (KeyValuePair<UserInfo, DateTime> pair in aliveUsers)
                    if (now.Subtract(pair.Value).TotalSeconds < pair.Key.KeepAliveSyncTime)
                        gone.Add(pair.Key);
                return gone; 
            }
        }

        void HeIsGone(UserInfo user)
        {
            lock (aliveUsers)
                aliveUsers.Remove(user); 
        }

        void HeIsAlive(UserInfo user)
        {
            lock (aliveUsers)
                aliveUsers[user] = DateTime.Now;
            lostUsers.Remove(user);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
