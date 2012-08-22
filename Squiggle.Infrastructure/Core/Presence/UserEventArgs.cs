using System;

namespace Squiggle.Core.Presence
{
    public class UserEventArgs : EventArgs
    {
        public IUserInfo User {get; set; }
    }
}
