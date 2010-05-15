using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat
{
    public interface IChatClient
    {
        event EventHandler<ChatStartedEventArgs> ChatStarted;
        event EventHandler<BuddyEventArgs> BuddyOnline;
        event EventHandler<BuddyEventArgs> BuddyOffline;

        Buddy CurrentUser{get; set;}
        List<Buddy> Buddies { get; set; }

        IChatSession StartChat(IPEndPoint endpoint);
        void Login(string username);
        void Logout();
    }
}
