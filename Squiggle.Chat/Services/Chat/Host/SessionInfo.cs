using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace Squiggle.Chat.Services.Chat.Host
{
    [DataContract]
    public class SessionInfo
    {
        [DataMember]
        public IPEndPoint[] Participants { get; set; }
    }
}
