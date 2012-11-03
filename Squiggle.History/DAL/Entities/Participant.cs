using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL.Entities
{
    public class Participant
    {
        public Guid Id {get; set;}
        public Guid SessionId { get; set; }
        public Guid ContactId { get; set; }
        public string ContactName { get; set; }

        public Session Session {get; set;}
    }
}
