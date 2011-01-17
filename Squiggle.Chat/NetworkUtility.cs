using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System;

namespace Squiggle.Chat
{
    public class NetworkUtility
    {
        public static int GetFreePort()
        {
            var listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static bool IsEndPointFree(IPEndPoint endpoint)
        {
            var listener = new TcpListener(endpoint);
            try
            {
                listener.Start();
                listener.Stop();
            }
            catch (SocketException)
            {
                return false;
            }
            return true;
        }

        public static bool IsValidIP(string address)
        {
            IPAddress ip;
            bool isValid = !String.IsNullOrEmpty(address) &&
                          (IPAddress.TryParse(address, out ip) && IsValidIP(ip));
            return isValid;
        }

        public static bool IsValidIP(IPAddress address)
        {
            bool isValid = GetLocalIPAddresses().Contains(address);
            return isValid;
        }

        public static IPAddress GetLocalIPAddress()
        {
            var address = GetLocalIPAddresses().FirstOrDefault();
            return address;
        }

        public static IEnumerable<IPAddress> GetLocalIPAddresses()
        {
            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in entry.AddressList)
            {
#if DEBUG
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
#else
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ip.Equals(IPAddress.Loopback))
#endif
                    yield return ip;
            }
        }
    }
}
