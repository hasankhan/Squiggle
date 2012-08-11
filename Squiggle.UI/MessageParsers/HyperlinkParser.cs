using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;
using Squiggle.UI.Helpers;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using System.ComponentModel.Composition;
using Squiggle.Plugins.MessageParser;

namespace Squiggle.UI.MessageParsers
{
    [Export(typeof(IMessageParser))]
    class HyperlinkParser : RegexParser
    {
        static Regex urlRegex = new Regex(@"(((ht|f)tp(s?)\:\/\/)|([w|W]{3}\.))[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9=:\-\.\?\,\'\/\\\+&%\$#_]*)?", RegexOptions.Compiled);

        protected override Regex Pattern
        {
            get { return urlRegex; }
        }
    
        protected override Inline Convert(string text)
        {
            var link = new Hyperlink(new Run(text));
            if (text.ToUpperInvariant().StartsWith("WWW"))
                text = "http://" + text;
            link.NavigateUri = new Uri(text, UriKind.Absolute);
            link.Cursor = Cursors.Hand;
            link.RequestNavigate += (s, e) =>
            {
                Shell.OpenUrl(link.NavigateUri.AbsoluteUri);
                e.Handled = true;
            };
            return link;
        }
    }
}
