using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Squiggle.Chat.Services.Presence
{    
    class UserDiscovery
    {
        private UdpClient reciever;
        private short broadcastPort;
        public event EventHandler<UserDiscoveredEventArgs> UserDiscovered = delegate { };        

        public UserDiscovery(short broadcastPort)
        {
            this.broadcastPort = broadcastPort;

            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, this.broadcastPort);
            this.reciever = new UdpClient(localEP);
           
            this.reciever.BeginReceive(new AsyncCallback(this.OnDataRecieved), null);
        }
        
        public void AnnouncePresence(UserInfo data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            UdpClient client = new UdpClient();
            using (MemoryStream stream = new MemoryStream())
            {
                UserInfo.Serialize(stream, data);
                AssureBroadcast(client, stream.ToArray());
                stream.Close();                
            }

            client.Close();
        }

        public void BroadcastKeepAlive()
        {
            UdpClient client = new UdpClient();
            AssureBroadcast(client, KeepAliveService.KeepAliveData);
            client.Close();
        }

        private void OnDataRecieved(IAsyncResult result)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, this.broadcastPort);
            byte[] buffer = this.reciever.EndReceive(result, ref remoteEndPoint);

            UserInfo data = null;
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                data = UserInfo.Deserialize(stream);
                stream.Close();
            }

            this.UserDiscovered(this, new UserDiscoveredEventArgs() { UserData = data });
            this.reciever.BeginReceive(new AsyncCallback(OnDataRecieved), null);
        }

        private void AssureBroadcast(UdpClient client, byte[] data)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, this.broadcastPort);            
            AssureSend(client, endPoint, data);
        }

        private void AssureSend(UdpClient client, IPEndPoint endPoint, byte[] data)
        {
            int bytesSent = 0;
            int length = data.Length;
            byte[] buffer = data;

            while (bytesSent < length)
            {
                bytesSent += client.Send(buffer, buffer.Length, endPoint);
                if (bytesSent < length)
                {
                    buffer = new byte[length - bytesSent];
                    Buffer.BlockCopy(data, bytesSent, buffer, 0, buffer.Length);
                }
            }
        }
    }
}
