using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Squiggle.Plugins.MessageParser
{
	public interface IMessageParser
	{
		bool TryParseText(string message, out MessageParseResult? result);
	}
}
