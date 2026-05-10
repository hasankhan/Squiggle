using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Squiggle.History.DAL.Entities
{
    public class Event
    {
        public string Id { get; set;  } = null!;
        public string SessionId { get; set; } = null!;
        public int TypeCode { get; set; }
        public string SenderId { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string Data { get; set; } = null!;
        public DateTime Stamp { get; set; }

        [NotMapped]
        public EventType Type
        {
            get { return (EventType)TypeCode; }
            set { TypeCode = (int)value; }
        }

        public Session Session { get; set; } = null!;
    }
}