using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Text.RegularExpressions;

namespace Squiggle.UI.MessageParsers
{
    public interface IMessageParser
    {
        Regex Pattern { get; }

        IEnumerable<Inline> ParseText(string text);
    }
}
