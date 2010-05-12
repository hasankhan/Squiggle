using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Service;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Chat
{
    public interface IChatSession
    {
        void SendMessage(string message);
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
