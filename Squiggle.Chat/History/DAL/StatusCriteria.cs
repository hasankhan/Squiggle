using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat.History.DAL
{
    public class StatusCriteria
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
