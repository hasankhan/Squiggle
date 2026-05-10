using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Squiggle.History.DAL.Entities
{
    public class Session
    {
        public string Id { get; set; } = null!;
        public string ContactId { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public ICollection<Event> Events { get; set; }
        public ICollection<Participant> Participants { get; set; }

        public Session()
        {
            Events = new Collection<Event>();
            Participants = new Collection<Participant>();
        }
    }
}
