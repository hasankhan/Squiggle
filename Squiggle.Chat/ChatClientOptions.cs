using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core;
using System.Net;

namespace Squiggle.Chat
{
    public class ChatClientOptions
    {
        public SquiggleEndPoint ChatEndPoint {get; set;}
        public IPEndPoint BroadcastEndPoint {get; set; }
        public IPEndPoint BroadcastReceiveEndPoint { get; set; }
        public IPEndPoint PresenceServiceEndPoint { get; set; }
        public TimeSpan KeepAliveTime { get; set; }
    }
}
