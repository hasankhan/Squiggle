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
        void SendMessage(string message);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        IPEndPoint RemoteUser { get; set; }
    }
}
