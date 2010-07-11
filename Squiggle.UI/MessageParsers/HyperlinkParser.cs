using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;

namespace Squiggle.UI.MessageParsers
{
    class HyperlinkParser: IMessageParser
    {
        static Regex urlRegex = new Regex(@"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9=:\-\.\?\,\'\/\\\+&%\$#_]*)?", RegexOptions.Compiled);

        public Regex Pattern
        {
            get { return urlRegex; }
        }

        public IEnumerable<Inline> ParseText(string text)
        {
            var link = new Hyperlink(new Run(text));
            link.NavigateUri = new Uri(text, UriKind.Absolute);
            link.Cursor = Cursors.Hand;
            link.RequestNavigate += (s, e) =>
            {
                Shell.OpenUrl(link.NavigateUri.AbsoluteUri);
                e.Handled = true;
            };
            yield return link;
        }
    }
}
