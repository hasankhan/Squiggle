using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace Squiggle.UI.MessageParsers
{
    class MultiParser: List<IMessageParser>
    {
        public IEnumerable<Inline> ParseText(string message)
        {
            var items = new List<Inline>();

            foreach (IMessageParser parser in this)
            {
                MessageParseResult result;
                if (parser.TryParseText(message, out result))
                {
                    if (!String.IsNullOrEmpty(result.Prefix))
                        items.AddRange(ParseText(result.Prefix));

                    items.Add(result.Converted);

                    if (!String.IsNullOrEmpty(result.Suffix))
                        items.AddRange(ParseText(result.Suffix));

                    return items;
                }
            }

            AddText(items, message);
            return items;
        }

        static void AddText(List<Inline> items, string text)
        {
            if (!String.IsNullOrEmpty(text))
                items.Add(new Run(text));
        }
    }
}
