using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Client
{
    public class ChatStartedEventArgs : EventArgs
    {
        public IBuddy Buddy
        {
            get { return Buddies.FirstOrDefault(); }
        }
        public IEnumerable<IBuddy> Buddies { get; set; }
        public IChat Chat { get; set; }
    }
}
