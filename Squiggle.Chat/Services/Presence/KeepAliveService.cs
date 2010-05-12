using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Timers;
using System.Net.Sockets;

namespace Squiggle.Chat.Services.Presence
{
    class KeepAliveService : IDisposable
    {
        public UserInfo User { get; private set; }
        private UdpClient client;
        private Timer timer;
        private bool dataRecieved = false;
        private short presencePort;
        private const int tolerance = 5000;

        ///Binary for "Alive"
        public static readonly byte[] KeepAliveData = new byte[] { 65, 108, 105, 118, 101 };
        public event EventHandler<UserLostEventArgs> UserLost = delegate { };

        public KeepAliveService(UserInfo user, short presencePort)
        {
            if (user.Address == null)
                throw new ArgumentNullException("UserData.Address");

            this.presencePort = presencePort;
            this.User = user;
        }

        public void StartServices()
        {
            ///Add some tolerance for network delays
            this.timer = new Timer(this.User.KeepAliveSyncTime + tolerance);
            this.timer.AutoReset = true;
            this.timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            this.timer.Start();

            IPEndPoint remoteEP = new IPEndPoint(this.User.Address, this.presencePort);
            this.client = new UdpClient(remoteEP);

            this.client.BeginReceive(new AsyncCallback(this.OnDataRecieved), remoteEP);
        }

        public void StopServices()
        {
            this.Dispose();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is KeepAliveService)
                return this.User.Equals(((KeepAliveService)obj).User);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.User.GetHashCode();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.dataRecieved)
            {
                this.Dispose();
                this.UserLost(this, new UserLostEventArgs() { Service = this });
            }
            this.dataRecieved = false;
        }

        private void OnDataRecieved(IAsyncResult result)
        {
            IPEndPoint remoteEP = (IPEndPoint)result.AsyncState;
            byte[] buffer = null;
            if (this.client != null)
            {
                lock (this.client)
                {
                    if (this.client != null)
                        buffer = this.client.EndReceive(result, ref remoteEP);
                }
            }

            if (buffer == null || (buffer != null && buffer.Length == 0))
            {
                return;
            }

            Console.WriteLine("Heart beat recieved");

            if (buffer.Length == KeepAliveData.Length)
            {
                byte[] diff = (byte[])buffer.Except(KeepAliveData);
                this.dataRecieved = diff.Length == 0;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Close();
                this.timer.Dispose();
                this.timer = null;
            }
            lock (this.client)
            {
                if (this.client != null)
                {
                    try
                    {
                        this.client.Close();
                    }
                    catch (SocketException)
                    {
                    }
                    this.client = null;
                }
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
