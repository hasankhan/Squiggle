using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat
{
    class UserInfo
    {
        public string Name { get; set; }
        public UserStatus Status { get; set; }
        public IPEndPoint ChatEndPoint { get; set; }
    }
}
