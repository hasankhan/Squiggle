using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    public interface IChatClient
    {
        IChatSession StartChat(string address);
        void Login(string username);
        void Logout();
    }
}
