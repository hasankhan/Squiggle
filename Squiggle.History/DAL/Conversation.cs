using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public IEnumerable<Participant> Participants { get; set; }
    }
}
