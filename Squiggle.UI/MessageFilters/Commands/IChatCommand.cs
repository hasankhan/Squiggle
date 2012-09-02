using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Plugins;
using Squiggle.UI.Components;

namespace Squiggle.UI.MessageFilters.Commands
{
    interface IChatCommand
    {
        bool IsMatch(string message);
        void Execute(string command, IChatWindow window, SquiggleContext context);
    }
}
