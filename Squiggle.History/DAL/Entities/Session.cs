using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Squiggle.History.DAL.Entities
{
    public class Session
    {
        public Guid Id { get; set; }
        public Guid ContactId { get; set; }
        public string ContactName { get; set; }
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
