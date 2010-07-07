using System;

namespace Squiggle.Chat
{
    public class BuddyEventArgs : EventArgs
    {
        public BuddyEventArgs() { }

        public BuddyEventArgs(Buddy buddy)
        {
            this.Buddy = buddy;
        }

        public Buddy Buddy { get; set; }
    }
}
