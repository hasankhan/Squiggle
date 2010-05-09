using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Net;
using Squiggle.Chat;

namespace Squiggle.Chat.Services.Presence
{
    class PresenceService : IPresenceService
    {
        private UserDiscovery discovery;
        private List<KeepAliveService> discoveredUsers;
        private Timer heartbeat;

        private short communicationPort = 11000;
        private IPAddress machineAddress;
        int keepAliveTime;

        public event EventHandler<UserEventArgs> UserOnline;
        public event EventHandler<UserEventArgs> UserOffline;

        public IEnumerable<UserInfo> Users
        {
            get { return discoveredUsers.Select(u => u.User); }
        }

        public PresenceService()
        {
            this.discovery = new UserDiscovery(communicationPort);
            this.discoveredUsers = new List<KeepAliveService>(10);

            if (this.machineAddress == null)
            {
                this.machineAddress = ((IPAddress[])Dns.GetHostAddresses(Environment.MachineName)).First(address =>
                    (address != IPAddress.Any && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));
            }
        }

        public PresenceService(IPAddress machineAddress, short communicationPort, int keepAliveTime)
            : this()
        {
            this.machineAddress = machineAddress;
            this.communicationPort = communicationPort;
            this.keepAliveTime = keepAliveTime;
        }

        void heartbeat_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.discovery.BroadcastKeepAlive();            
        }

        void discovery_OnUserDiscovered(object sender, UserDiscoveredEventArgs args)
        {
            if (args.UserData != null)
            {
                KeepAliveService service = new KeepAliveService(args.UserData);
                service.UserLost += new EventHandler<UserLostEventArgs>(service_OnUserLost);
                lock (this.discoveredUsers)
                {
                    if (!this.discoveredUsers.Contains(service))
                    {
                        this.discoveredUsers.Add(service);
                        service.StartServices();
                        UserOnline(this, new UserEventArgs() { User = args.UserData });
                    }
                }
            }
        }

        void service_OnUserLost(object sender, UserLostEventArgs args)
        {
            if (args.Service != null)
            {
                lock (this.discoveredUsers)
                    this.discoveredUsers.Remove(args.Service);
                args.Service.StopServices();
                UserOffline(this, new UserEventArgs() { User = args.Service.User });
            }
        }                

        public void Login(string friendlyName)
        {
            UserInfo data = new UserInfo()
            {
                UserFriendlyName = friendlyName,
                Address = this.machineAddress,
                Port = 12000,
                KeepAliveSyncTime = keepAliveTime
            };

            discovery.AnnouncePrecense(data);
            discovery.UserDiscovered += new EventHandler<UserDiscoveredEventArgs>(discovery_OnUserDiscovered);

            this.heartbeat = new Timer(keepAliveTime);
            this.heartbeat.AutoReset = true;
            this.heartbeat.Elapsed += new ElapsedEventHandler(heartbeat_Elapsed);
            this.heartbeat.Start();
        }

        public void Logout()
        {
            this.Dispose();
        }

        #region IDisposable Members

        public void Dispose()
        {
            lock (this.discoveredUsers)
            {
                this.discoveredUsers.ForEach(service => service.StopServices());
                this.discoveredUsers.Clear();
            }

            if (this.heartbeat != null)
            {
                this.heartbeat.Stop();
                this.heartbeat.Close();
                this.heartbeat.Dispose();
                this.heartbeat = null;
            }
        }

        #endregion
    }    
}
