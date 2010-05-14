using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace Squiggle.Chat
{
    class Program
    {
        static void Main(string[] args)
        {
            ChatClient client1 = new ChatClient(new IPEndPoint(IPAddress.Loopback, 1234), 12345, 2);
            ChatClient client2 = new ChatClient(new IPEndPoint(IPAddress.Loopback, 1236), 12345, 2);
            client1.BuddyOnline += new EventHandler<BuddyEventArgs>(client_BuddyOnline);
            client2.BuddyOnline += new EventHandler<BuddyEventArgs>(client_BuddyOnline);
            client2.BuddyOffline += new EventHandler<BuddyEventArgs>(client2_BuddyOffline);
            client1.Login("hasan");            
            client2.Login("Ali");
            client1.Logout();
            Console.ReadLine();
        }

        static void client2_BuddyOffline(object sender, BuddyEventArgs e)
        {
            Console.WriteLine("Offline {0}", e.Buddy.DisplayName);            
        }

        static void client_BuddyOnline(object sender, BuddyEventArgs e)
        {
            Console.WriteLine("Online {0}", e.Buddy.DisplayName);
        }
    }
}
