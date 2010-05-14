using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    public interface IChatClient
    {
        event EventHandler<ChatStartedEventArgs> ChatStarted;
        event EventHandler<BuddyEventArgs> BuddyOnline;
        event EventHandler<BuddyEventArgs> BuddyOffline;

        IChatSession StartChat(string address);
        void Login(string username);
        void Logout();
    }
}
