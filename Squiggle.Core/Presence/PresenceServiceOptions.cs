using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Core.Presence
{
    public class PresenceServiceOptions
    {
        public SquiggleEndPoint ChatEndPoint {get; set; } = null!;
        public IPEndPoint MulticastEndPoint {get; set; } = null!;
        public IPEndPoint MulticastReceiveEndPoint { get; set; } = null!;
        public IPEndPoint PresenceServiceEndPoint {get; set; } = null!; 
        public TimeSpan KeepAliveTime {get; set; }
    }
}
