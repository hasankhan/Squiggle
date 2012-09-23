using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Windows;
using Squiggle.Client;
using Squiggle.UI.Resources;

namespace Squiggle.UI.MessageFilters.Commands
{
    class InviteCommand: PrefixCommand
    {
        protected override string Prefix
        {
            get { return "/INVITE"; }
        }

        protected override void OnExecute(string argument, Plugins.IChatWindow window, Components.SquiggleContext context)
        {
            string displayName = argument.Trim();
            IBuddy buddy = context.ChatClient.Buddies.FirstOrDefault(b => b.DisplayName.Trim().Equals(displayName, StringComparison.InvariantCultureIgnoreCase));
            if (buddy == null)
                ((ChatWindow)window).chatTextBox.AddError(String.Format(Translation.Instance.ChatWindow_NoBuddyWithName, argument), String.Empty);
            else
                window.Invite(buddy);
        }
    }
}
