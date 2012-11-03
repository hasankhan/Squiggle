using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Squiggle.History.DAL.Entities
{
    public class Event
    {
        public Guid Id { get; set;  }
        public Guid SessionId { get; set; }
        public int TypeCode { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string Data { get; set; }
        public DateTime Stamp { get; set; }

        [NotMapped]
        public EventType Type
        {
            get { return (EventType)TypeCode; }
            set { TypeCode = (int)value; }
        }

        public Session Session { get; set; }
    }
}