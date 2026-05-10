using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL.Entities
{
    public class Participant
    {
        public string Id {get; set;} = null!;
        public string SessionId { get; set; } = null!;
        public string ContactId { get; set; } = null!;
        public string ContactName { get; set; } = null!;

        public Session Session {get; set;} = null!;
    }
}
