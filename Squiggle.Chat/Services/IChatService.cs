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
        public string Address { get; set; }
        public IChatSession Session {get; set; }
    }

    public interface IChatService
    {
        void Start(IPEndPoint endpoint);
        void Stop();
        IChatSession CreateSession(IPEndPoint host);
        event EventHandler<ChatStartedEventArgs> ChatStarted;
        event EventHandler<ResolveEndPointEventArgs> ResolveEndPoint;
    }
}
