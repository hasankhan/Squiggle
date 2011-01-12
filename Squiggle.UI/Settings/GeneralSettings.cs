
using System.Collections.Specialized;
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

        public GeneralSettings()
        {
            ShowEmoticons = true;
            HideToSystemTray = true;
            ShowPopups = true;
            AudioAlerts = false;
            ContactListSortField = "DisplayName";
            GroupContacts = true;
            SpellCheck = true;
            ContactGroups = new ContactGroups();
        }
    }
}
