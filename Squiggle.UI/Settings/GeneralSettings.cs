
using System.Collections.Specialized;
using System;
namespace Squiggle.UI.Settings
{
    class GeneralSettings
    {
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public bool AudioAlerts { get; set; }
        public string ContactListSortField { get; set; }
        public bool SpellCheck { get; set; }
        public string DownloadsFolder { get; set; }
        public bool ShowEmoticons { get; set; }
        public bool GroupContacts { get; set; }
        public ContactGroups ContactGroups { get; set; }
        public bool MinimizeChatWindows { get; set; }
        public bool EnableLogging { get; set; }
        public DateTimeOffset FirstRun { get; set; }

        public GeneralSettings()
        {
            ShowEmoticons = true;
            HideToSystemTray = true;
            ShowPopups = true;
            AudioAlerts = false;
            ContactListSortField = "DisplayName";
            GroupContacts = true;
            SpellCheck = true;
            EnableLogging = true;
            ContactGroups = new ContactGroups();
        }
    }
}
