using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Squiggle.UI.Windows;

namespace Squiggle.UI.MessageFilters.Commands
{
    class UpdateMessageCommand: PrefixCommand
    {
        protected override string Prefix
        {
            get { return "/UPDATE "; }
        }

        protected override void OnExecute(string argument, Plugins.IChatWindow window, Components.SquiggleContext context)
        {
            ((ChatWindow)window).UpdateLastMessage(argument);
        }
    }
}
