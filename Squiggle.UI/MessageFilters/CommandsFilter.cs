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

namespace Squiggle.UI.MessageFilters
{
    [Export(typeof(IMessageFilter))]
    class CommandsFilter : IMessageFilter
    {
        public FilterDirection Direction
        {
            get { return FilterDirection.Out; }
        }

        static Dictionary<string, Action<IChatWindow>> simpleCommands = new Dictionary<string, Action<IChatWindow>>();

        static CommandsFilter()
        {
            var context = SquiggleContext.Current;

            simpleCommands["CLS"] = window => ((ChatWindow)window).chatTextBox.Clear();
            simpleCommands["/QUIT"] = window => context.MainWindow.Quit();
            simpleCommands["/ONLINE"] = window => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.Online;
            simpleCommands["/OFFLINE"] = window => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.Offline;
            simpleCommands["/AWAY"] = window => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.Away;
            simpleCommands["/BRB"] = window => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.BeRightBack;
            simpleCommands["/BUSY"] = window => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.Busy;
            simpleCommands["/BUZZ"] = window => window.SendBuzz();
            simpleCommands["/MAIN"] = window => context.MainWindow.RestoreWindow();
        }

        public bool Filter(StringBuilder message, IChatWindow window)
        {
            string command = message.ToString().Trim().ToUpperInvariant();

            Action<IChatWindow> action;
            if (simpleCommands.TryGetValue(command, out action))
            {
                action(window);
                return false;
            }

            return true;
        }
    }
}
