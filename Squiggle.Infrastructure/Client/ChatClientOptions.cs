using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core;
using System.Net;
using Squiggle.Core.Presence;

namespace Squiggle.Client
{
    public class ChatClientOptions
    {
        public ISquiggleEndPoint ChatEndPoint {get; set;}
        public IPEndPoint MulticastEndPoint {get; set; }
        public IPEndPoint MulticastReceiveEndPoint { get; set; }
        public IPEndPoint PresenceServiceEndPoint { get; set; }
        public TimeSpan KeepAliveTime { get; set; }
        
        public string Username { get;set; }
        public IBuddyProperties UserProperties {get; set; } 
    }
}
