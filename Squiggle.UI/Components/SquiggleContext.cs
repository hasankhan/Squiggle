using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Windows;
using Squiggle.Client;
using Squiggle.Core.Chat.Activity;
using Squiggle.Client.Activities;
using Squiggle.Plugins;

namespace Squiggle.UI.Components
{
    class SquiggleContext : ISquiggleContext
    {
        public PluginLoader PluginLoader { get; set; } = null!;
        public IMainWindow MainWindow { get; set; } = null!;
        public IChatClient ChatClient { get; set; } = null!;
        public IVoiceChatHandler? ActiveVoiceChat { get; set; }
        public bool IsVoiceChatActive
        {
            get { return ActiveVoiceChat != null; }
        }

        public static SquiggleContext? Current { get; set; }
    }
}
