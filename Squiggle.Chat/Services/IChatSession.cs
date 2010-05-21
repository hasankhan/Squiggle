using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Chat.Host;
using System.Net;

namespace Squiggle.Chat.Services
{
    public interface IChatSession
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<UserEventArgs> UserTyping;

        IPEndPoint RemoteUser { get; set; }

        void SendMessage(string message);        
        void NotifyTyping();
        void End();
    }
}
