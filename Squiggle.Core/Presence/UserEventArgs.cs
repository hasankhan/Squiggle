using System;

namespace Squiggle.Core.Presence
{
    public class UserEventArgs : EventArgs
    {
        public UserInfo User {get; set; }
    }
}
