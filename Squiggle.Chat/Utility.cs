using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat
{
    public class Utility
    {
        public static IPAddress GetLocalIPAddress()
        {
            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in entry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip;
            }

            return entry.AddressList[0];
        }
    }
}
