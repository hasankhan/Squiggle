using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL.Entities
{
    public enum EventType: int
    {
        Message = 0,
        Buzz = 1,
        Joined = 2,
        Left = 3,
        Activity = 4
    }
}
