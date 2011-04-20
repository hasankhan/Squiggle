using System;
using System.Net;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat.Services
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
