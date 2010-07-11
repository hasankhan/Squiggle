
namespace Squiggle.UI.Settings
{
    class GeneralSettings
    {
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public string ContactListSortField { get; set; }
        public bool SpellCheck { get; set; }
        public string DownloadsFolder { get; set; }
        public bool ShowEmoticons { get; set; }

        public GeneralSettings()
        {
            ShowEmoticons = true;
            HideToSystemTray = true;
            ShowPopups = true;
            ContactListSortField = "DisplayName";
            SpellCheck = true;
        }
    }
}
