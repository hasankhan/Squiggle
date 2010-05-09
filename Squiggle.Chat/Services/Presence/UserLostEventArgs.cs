using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat.Services.Presence
{
    class UserLostEventArgs : EventArgs
    {
        public KeepAliveService Service = null;        
    }
}
