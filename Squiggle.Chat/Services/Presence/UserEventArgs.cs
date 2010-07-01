using System;

namespace Squiggle.Chat.Services.Presence
{
    class UserEventArgs : EventArgs
    {
        public UserInfo User {get; set; }
    }
}
