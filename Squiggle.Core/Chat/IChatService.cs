using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

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
        IEnumerable<IChatSession> Sessions { get; }
        IChatSession CreateSession(SquiggleEndPoint endpoint);
        event EventHandler<ChatStartedEventArgs> ChatStarted;
    }
}
