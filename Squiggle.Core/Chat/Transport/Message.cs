using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat.Transport.Messages;

namespace Squiggle.Core.Chat.Transport
{
    public abstract class Message
    {
        public Guid SessionId { get; set; }
        /// <summary>
        /// Chat endpoint for the sender
        /// </summary>
        public SquiggleEndPoint Sender { get; set; } = null!;

        /// <summary>
        /// Chat endpoint for the recipient
        /// </summary>
        public SquiggleEndPoint Recipient { get; set; } = null!;
    }
}
