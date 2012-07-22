using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Squiggle.Utilities.Net
{
    public class NetworkUtility
    {
        public static bool IsMulticast(IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                byte first = ip.GetAddressBytes()[0];
                return (first >= 224 && first <= 239);
            }
            else
                return ip.IsIPv6Multicast;
        }  

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

        public static bool TryParseAddress(string address, out IPAddress ip)
        {
            if (IPAddress.TryParse(address, out ip))
                return true;
            else
            {
                try
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(address);
                    if (addresses.Any())
                    {
                        ip = addresses.First();
                        return true;
                    }
                }
                catch (SocketException) { }
            }
            return false;
        }

        public static bool IsValidLocalIP(string address)
        {
            IPAddress ip;
            bool isValid = !String.IsNullOrEmpty(address) &&
                          (IPAddress.TryParse(address, out ip) && IsValidLocalIP(ip));
            return isValid;
        }

        public static bool IsValidLocalIP(IPAddress address)
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

                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
#if !DEBUG
                    && !ip.Equals(IPAddress.Loopback)
#endif
                    )
                    yield return ip;
            }
        }
    }
}
