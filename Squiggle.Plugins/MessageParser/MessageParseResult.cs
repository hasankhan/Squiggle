using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace Squiggle.Plugins.MessageParser
{
    public class MessageParseResult
    {
        public string Prefix { get; set; }
        public Inline Converted { get; private set; }
        public string Suffix { get; set; }

        public MessageParseResult(Inline converted)
        {
            this.Converted = converted;
        }
    }
}
