
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
        public string DownloadsFolder { get; set; }
        public bool GroupContacts { get; set; }
        public ContactGroups ContactGroups { get; set; }
        public bool MinimizeChatWindows { get; set; }
        public DateTimeOffset FirstRun { get; set; }

        public GeneralSettings()
        {
            HideToSystemTray = true;
            ShowPopups = true;
            AudioAlerts = false;
            ContactListSortField = "DisplayName";
            GroupContacts = true;
            ContactGroups = new ContactGroups();
        }
    }
}
