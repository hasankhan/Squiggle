using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace Squiggle.UI.MessageParsers
{
	public class MessageParseResult
	{
		public string Prefix { get; set; }
		public Inline Converted { get; private set; }
		public string Suffix { get; set; }

		public MessageParseResult (Inline converted)
		{
			this.Converted = converted;
		}
	}

	public interface IMessageParser
	{
		bool TryParseText(string message, out MessageParseResult result);
	}
}
