using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Client
{
    public class BuddyOnlineEventArgs : BuddyEventArgs
    {
        public bool Discovered { get; set; }
    }
}
