using System;
using System.Collections.Generic;
using System.Timers;
using Squiggle.Core.Presence.Transport;
using Squiggle.Core.Presence.Transport.Messages;
using Squiggle.Utilities;
using System.Diagnostics;
using System.Linq;

namespace Squiggle.Core.Presence
{
    class KeepAliveService : IDisposable
    {
        Timer timer;
        PresenceChannel channel;
        TimeSpan keepAliveSyncTime;
        Message keepAliveMessage;
        Dictionary<UserInfo, DateTime> aliveUsers;
        DateTime lastKeepAliveMessage;

        public event EventHandler<UserEventArgs> UserLost = delegate { };
        public event EventHandler<UserEventArgs> UserLosing = delegate { };
        public event EventHandler<UserEventArgs> UserDiscovered = delegate { };

        public KeepAliveService(PresenceChannel channel, UserInfo user, TimeSpan keepAliveSyncTime)
        {
            this.channel = channel;
            this.keepAliveSyncTime = keepAliveSyncTime;
            keepAliveMessage = Message.FromSender<KeepAliveMessage>(user);
            aliveUsers = new Dictionary<UserInfo, DateTime>();
        }

        public void Start()
        {
            this.timer = new Timer();
            timer.Interval = keepAliveSyncTime.TotalMilliseconds;
            this.timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            this.timer.Start();
            channel.MessageReceived += new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);
        }

        void channel_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is KeepAliveMessage)
                OnKeepAliveMessage((KeepAliveMessage)e.Message);
        }        

        public void MonitorUser(UserInfo user)
        {
            HeIsAlive(user);
        }

        public void LeaveUser(UserInfo user)
        {
            HeIsGone(user);
        }

        public void HeIsGone(UserInfo user)
        {
            lock (aliveUsers)
                aliveUsers.Remove(user);
        }

        public void HeIsAlive(UserInfo user)
        {
            lock (aliveUsers)
                aliveUsers[user] = DateTime.Now;   
        }

        public void Stop()
        {
            channel.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(channel_MessageReceived);

            lock (aliveUsers)
                aliveUsers.Clear();

            timer.Stop();
            timer = null;
        }

        void ImAlive()
        {
            channel.SendMulticastMessage(keepAliveMessage);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((DateTime.UtcNow - lastKeepAliveMessage).TotalMilliseconds < timer.Interval / 2)
                return;

            lastKeepAliveMessage = DateTime.UtcNow;
            ImAlive();

            List<UserInfo> gone = GetLostUsers(keepAliveTime => (keepAliveTime + keepAliveTime) + 10.Seconds());
            gone.ForEach(user =>
            {
                HeIsGone(user);
                UserLost(this, new UserEventArgs() { User = user });
            });

            List<UserInfo> going = GetLostUsers(keepAliveTime =>keepAliveTime + 5.Seconds()).Except(gone).ToList();
            going.ForEach(user=>UserLosing(this, new UserEventArgs(){User = user }));

            
        }        

        void OnKeepAliveMessage(KeepAliveMessage message)
        {
            var user = new UserInfo() { ID = message.ClientID,
                                        PresenceEndPoint = message.PresenceEndPoint };
            bool existingUser;
            lock (aliveUsers)
                existingUser = aliveUsers.ContainsKey(user);

            if (existingUser)
                HeIsAlive(user);
            else
                UserDiscovered(this, new UserEventArgs() { User = user });
        }

        List<UserInfo> GetLostUsers(Func<TimeSpan, TimeSpan> waitTimeSelector)
        {
            lock (aliveUsers)
            {
                var now = DateTime.Now;
                List<UserInfo> gone = new List<UserInfo>();
                foreach (KeyValuePair<UserInfo, DateTime> pair in aliveUsers)
                {
                    TimeSpan inactiveTime = now.Subtract(pair.Value);
                    TimeSpan waitTime = waitTimeSelector(pair.Key.KeepAliveSyncTime);
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
