using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    public class ChatStartedEventArgs: EventArgs
    {
        public Buddy Buddy { get; set; }
        public IChat Chat { get; set; }
        public string Message { get; set; }
    }
}
