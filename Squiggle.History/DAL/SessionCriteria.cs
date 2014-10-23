using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL
{
    public class SessionCriteria
    {
        public string SessionId { get; set; }
        public string Participant { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Text { get; set; }
    }
}
