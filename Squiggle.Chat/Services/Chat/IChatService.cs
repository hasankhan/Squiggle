using System;
using System.Net;

namespace Squiggle.Chat.Services.Chat
{
    public class ChatStartedEventArgs: EventArgs
    {
        public IChatSession Session {get; set; }
    }

    public interface IChatService
    {
        void Start();
        void Stop();
        IChatSession CreateSession(SquiggleEndPoint endpoint);
        event EventHandler<ChatStartedEventArgs> ChatStarted;
    }
}
