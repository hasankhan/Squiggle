using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.MessageFilters.Commands
{
    abstract class PrefixCommand: IChatCommand
    {
        protected abstract string Prefix { get; }

        public bool IsMatch(string message)
        {
            return message.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Execute(string command, Plugins.IChatWindow window, Components.SquiggleContext context)
        {
            command = command.Substring(Prefix.Length);
            OnExecute(command, window, context);
        }

        protected abstract void OnExecute(string command, Plugins.IChatWindow window, Components.SquiggleContext context);
    }
}
