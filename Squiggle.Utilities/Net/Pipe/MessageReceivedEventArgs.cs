using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Utilities.Net.Pipe
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public byte[] Message { get; set; }
    }
}
