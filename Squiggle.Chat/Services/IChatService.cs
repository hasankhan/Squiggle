using System;
using System.Net;

namespace Squiggle.Chat.Services
{
    public class ChatStartedEventArgs: EventArgs
    {
        public IChatSession Session {get; set; }
    }

    public interface IChatService
    {
        string Username { get; set; }
        void Start(IPEndPoint endpoint);
        void Stop();
        IChatSession CreateSession(IPEndPoint endpoint);
        event EventHandler<ChatStartedEventArgs> ChatStarted;
    }
}
