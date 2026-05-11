using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Plugins.MessageParser
{
    public class MessageParseResult
    {
        public string? Prefix { get; set; }
        public object Converted { get; private set; }
        public string? Suffix { get; set; }

        public MessageParseResult(object converted)
        {
            this.Converted = converted;
        }
    }
}
