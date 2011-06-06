using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat.History.DAL
{
    partial class Event
    {
        public EventType Type
        {
            get { return (EventType)EventTypeCode; }
            set { EventTypeCode = (int)value; }
        }
    }
}
