using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat.History.DAL
{
    public enum EventType
    {
        Message,
        Buzz,
        Joined,
        Left,
        Transfer,
        Voice,
    }
}
