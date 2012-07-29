using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Squiggle.Core.Chat
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
