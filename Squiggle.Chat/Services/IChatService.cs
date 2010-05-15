using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat
{
    public class ChatStartedEventArgs: EventArgs
    {
        public IChatSession Session {get; set; }
        public Buddy Buddy { get; set; }
        public string Message { get; set; }
    }

    public interface IChatService
    {
        string Username { get; set; }
        void Start(IPEndPoint endpoint);
        void Stop();
        IChatSession CreateSession(IPEndPoint endpoint);
        void RemoveSession(IPEndPoint endpoint);
        event EventHandler<ChatStartedEventArgs> ChatStarted;
    }
}
