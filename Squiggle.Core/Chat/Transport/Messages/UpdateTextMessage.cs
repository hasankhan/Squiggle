using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat.Transport.Messages
{
    class UpdateTextMessage : Message
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = null!;
    }
}
