using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence;

namespace Squiggle.Chat
{
    public class Buddy
    {
        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public string Address { get; internal set; }
        public UserStatus Status { get; set; }
    }
}
