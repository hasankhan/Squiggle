using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace Squiggle.Plugins.MessageParser
{
	public interface IMessageParser
	{
		bool TryParseText(string message, out MessageParseResult result);
	}
}
