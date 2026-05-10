using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core;
using System.Net;
using Squiggle.Core.Presence;

namespace Squiggle.Client
{
    public class LoginOptions
    {
        public IPEndPoint ChatEndPoint {get; set;} = null!;
        public IPEndPoint MulticastEndPoint {get; set; } = null!;
        public IPEndPoint MulticastReceiveEndPoint { get; set; } = null!;
        public IPEndPoint PresenceServiceEndPoint { get; set; } = null!;
        public TimeSpan KeepAliveTime { get; set; }
        
        public string DisplayName { get;set; } = null!;
        public IBuddyProperties UserProperties {get; set; } = null!; 
    }
}
