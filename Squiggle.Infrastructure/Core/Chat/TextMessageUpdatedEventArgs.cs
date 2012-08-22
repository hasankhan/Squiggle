using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat
{
    public class TextMessageUpdatedEventArgs : SessionEventArgs
    {
        public string Id { get; set; }
        public string Message { get; set; }
    }
}
