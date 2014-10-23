using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL.Entities
{
    public class StatusUpdate
    {
        public string Id { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public int StatusCode { get; set; }
        public DateTime Stamp { get; set; }
    }
}
