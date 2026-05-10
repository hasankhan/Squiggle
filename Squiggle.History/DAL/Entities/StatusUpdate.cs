using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL.Entities
{
    public class StatusUpdate
    {
        public string Id { get; set; } = null!;
        public string ContactId { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public int StatusCode { get; set; }
        public DateTime Stamp { get; set; }
    }
}
