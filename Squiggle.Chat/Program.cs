using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

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
            client1.Login("hasan");            
            client2.Login("Ali");
            Console.ReadLine();
        }

        static void client_BuddyOnline(object sender, BuddyEventArgs e)
        {
            Console.WriteLine("{0} {1}", sender.ToString(), e.Buddy.DisplayName);
        }
    }
}
