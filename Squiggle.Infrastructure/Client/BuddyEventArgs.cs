using System;

namespace Squiggle.Client
{
    public class BuddyEventArgs : EventArgs
    {
        public BuddyEventArgs() { }

        public BuddyEventArgs(IBuddy buddy)
        {
            this.Buddy = buddy;
        }

        public IBuddy Buddy { get; set; }
    }
}
