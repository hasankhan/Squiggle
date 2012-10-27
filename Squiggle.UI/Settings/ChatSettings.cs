using Squiggle.Utilities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Settings
{
    class ChatSettings : ISettingsGroup
    {
        public bool EnableLogging { get; set; }
        public bool ShowEmoticons { get; set; }
        public bool SpellCheck { get; set; }
        public bool StealFocusOnNewMessage { get; set; }
        public bool ClearChatOnWindowClose { get; set; }
        public int MaxMessagesToPreserve { get; set; }

        public void ReadFrom(Properties.Settings settings, ConfigReader reader)
        {
            ShowEmoticons = settings.ShowEmoticons;
            SpellCheck = settings.SpellCheck;
            StealFocusOnNewMessage = settings.StealFocusOnNewMessage;
            ClearChatOnWindowClose = settings.ClearChatOnWindowClose;
            EnableLogging = reader.GetSetting(SettingKey.EnableChatLogging, false);
            MaxMessagesToPreserve = reader.GetSetting(SettingKey.MaxMessagesToPreserve, 100);
        }

        public void WriteTo(Properties.Settings settings, ConfigReader reader)
        {
            settings.ShowEmoticons = ShowEmoticons;
            settings.SpellCheck = SpellCheck;
            settings.StealFocusOnNewMessage = StealFocusOnNewMessage;
            settings.ClearChatOnWindowClose = ClearChatOnWindowClose;
            reader.SetSetting(SettingKey.EnableChatLogging, EnableLogging);
        }
    }
}
