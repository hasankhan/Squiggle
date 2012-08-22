using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat
{
    public class SessionEventArgs : EventArgs
    {
        public Guid SessionID { get; set; }
        public ISquiggleEndPoint Sender { get; set; }

        public SessionEventArgs() { }

        public SessionEventArgs(Guid sessionId, ISquiggleEndPoint user)
        {
            this.SessionID = sessionId;
            this.Sender = user;
        }
    }
}
