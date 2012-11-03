using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL.Entities
{
    public class StatusUpdate
    {
        public Guid Id { get; set; }
        public Guid ContactId { get; set; }
        public string ContactName { get; set; }
        public int StatusCode { get; set; }
        public DateTime Stamp { get; set; }
    }
}
