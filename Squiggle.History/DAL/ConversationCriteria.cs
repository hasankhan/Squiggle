using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL
{
    public class ConversationCriteria
    {
        public Guid? SessionId { get; set; }
        public Guid? Participant { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Text { get; set; }
    }
}
