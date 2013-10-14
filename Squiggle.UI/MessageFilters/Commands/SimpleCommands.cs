using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Plugins;
using Squiggle.UI.Components;
using Squiggle.UI.Windows;

namespace Squiggle.UI.MessageFilters.Commands
{
    class SimpleCommands: IChatCommand
    {
        static Dictionary<string, Action<IChatWindow, SquiggleContext>> commands = new Dictionary<string, Action<IChatWindow, SquiggleContext>>();

        public SimpleCommands()
        {
            commands["CLS"] = (window, context) => ((ChatWindow)window).chatTextBox.Clear();
            commands["/QUIT"] = (window, context) => context.MainWindow.Quit();
            commands["/ONLINE"] = (window, context) => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.Online;
            commands["/OFFLINE"] = (window, context) => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.Offline;
            commands["/AWAY"] = (window, context) => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.Away;
            commands["/BRB"] = (window, context) => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.BeRightBack;
            commands["/BUSY"] = (window, context) => context.ChatClient.CurrentUser.Status = Core.Presence.UserStatus.Busy;
            commands["/BUZZ"] = (window, context) => window.SendBuzz();
            commands["/MAIN"] = (window, context) => context.MainWindow.Restore();
        }

        public bool IsMatch(string message)
        {
            return commands.ContainsKey(message.ToUpperInvariant());
        }

        public void Execute(string command, Plugins.IChatWindow window, Components.SquiggleContext context)
        {
            Action<IChatWindow, SquiggleContext> action;
            if (commands.TryGetValue(command.ToUpperInvariant(), out action))
                action(window, context);
        }
    }
}
