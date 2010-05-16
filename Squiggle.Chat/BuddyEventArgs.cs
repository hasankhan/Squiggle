using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    public class BuddyEventArgs : EventArgs
    {
        public Buddy Buddy { get; set; }
    }
}
