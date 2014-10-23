using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL.Entities
{
    public class Participant
    {
        public string Id {get; set;}
        public string SessionId { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }

        public Session Session {get; set;}
    }
}
