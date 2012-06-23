using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace Squiggle.Core.Chat.Host
{
    [DataContract]
    public class SessionInfo
    {
        [DataMember]
        public SquiggleEndPoint[] Participants { get; set; }
    }
}
