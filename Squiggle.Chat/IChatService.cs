using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat
{
    class ChatStartedEventArgs: EventArgs
    {
        public IChatSession Session {get; set; }
    }

    interface IChatService
    {
        void Start(IPEndPoint endpoint);
        void Stop();
        IChatSession CreateSession(IPEndPoint host, string remoteUser);
        event EventHandler<ChatStartedEventArgs> ChatStarted;
    }
}
