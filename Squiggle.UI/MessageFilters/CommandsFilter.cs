using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Client;
using Squiggle.UI.Windows;
using System.ComponentModel.Composition;
using Squiggle.UI.Components;
using Squiggle.Plugins.MessageFilter;
using Squiggle.Plugins;
using Squiggle.UI.MessageFilters.Commands;

namespace Squiggle.UI.MessageFilters
{
    [Export(typeof(IMessageFilter))]
    class CommandsFilter : IMessageFilter
    {
        static List<IChatCommand> commands;

        public FilterDirection Direction
        {
            get { return FilterDirection.Out; }
        }

        static CommandsFilter()
        {
            var context = SquiggleContext.Current;
            commands = new List<IChatCommand>()
            { 
                new SimpleCommands(), 
                new UpdateMessageCommand() 
            };
        }

        public bool Filter(StringBuilder message, IChatWindow window)
        {
            string text = message.ToString();
            IChatCommand command = commands.FirstOrDefault(cmd => cmd.IsMatch(text));
            if (command == null)
                return true;

            command.Execute(text, window, SquiggleContext.Current);

            return false;
        }
    }
}
