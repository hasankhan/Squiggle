using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Timers;
using System.Net.Sockets;
using Squiggle.Chat.Services.Presence.Transport;
using Squiggle.Chat.Services.Presence.Transport.Messages;

namespace Squiggle.Chat.Services.Presence
{
    class KeepAliveService : IDisposable
    {
        Timer timer;
        PresenceChannel channel;
        TimeSpan keepAliveSyncTime;
        Message keepAliveMessage;
        Dictionary<UserInfo, DateTime> aliveUsers;
        HashSet<UserInfo> lostUsers;

        public UserInfo User { get; private set; }

        public event EventHandler<UserEventArgs> UserLost = delegate { };
        public event EventHandler<UserEventArgs> UserReturned = delegate { };

        public KeepAliveService(PresenceChannel channel, UserInfo user, TimeSpan keepAliveSyncTime)
        {
            this.channel = channel;
            this.keepAliveSyncTime = keepAliveSyncTime;
            this.User = user;
            aliveUsers = new Dictionary<UserInfo, DateTime>();
            lostUsers = new HashSet<UserInfo>();
        }

        public void Start()
        {
            this.timer = new Timer();
            timer.Interval = keepAliveSyncTime.TotalMilliseconds;
            this.timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            this.timer.Start();
            channel.MessageReceived += new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);
            keepAliveMessage = new KeepAliveMessage() { ChatEndPoint = User.ChatEndPoint };
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
            lock (aliveUsers)
                lostUsers.Remove(user);
        }

        public void HeIsGone(UserInfo user)
        {
            lock (aliveUsers)
                aliveUsers.Remove(user);
        }

        public void HeIsAlive(UserInfo user)
        {
            lock (aliveUsers)
            {
                aliveUsers[user] = DateTime.Now;
                lostUsers.Remove(user);
            }
        }

        public void Stop()
        {
            channel.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);
            lostUsers.Clear();
            aliveUsers.Clear();

            timer.Stop();
            timer = null;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ImAlive();

            lock (aliveUsers)
            {
                List<UserInfo> gone = GetLostUsers();

                foreach (UserInfo user in gone)
                {
                    lostUsers.Add(user);
                    HeIsGone(user);
                    UserLost(this, new UserEventArgs() { User = user });
                }
            }
        }        

        void OnKeepAliveMessage(KeepAliveMessage message)
        {
            var user = new UserInfo() { ChatEndPoint = message.ChatEndPoint };
            HeIsAlive(user);
        }

        List<UserInfo> GetLostUsers()
        {
            lock (aliveUsers)
            {
                var now = DateTime.Now;
                List<UserInfo> gone = new List<UserInfo>();
                foreach (KeyValuePair<UserInfo, DateTime> pair in aliveUsers)
                {
                    TimeSpan inactiveTime = now.Subtract(pair.Value);
                    var tolerance = pair.Key.KeepAliveSyncTime + 5.Seconds();
                    TimeSpan waitTime = pair.Key.KeepAliveSyncTime + tolerance;
                    if (inactiveTime > waitTime)
                        gone.Add(pair.Key);
                }
                return gone; 
            }
        }        

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
