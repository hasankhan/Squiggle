
using System;
using System.Collections.Specialized;
namespace Squiggle.UI.Settings
{
    class GeneralSettings
    {
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public bool AudioAlerts { get; set; }
        public string DownloadsFolder { get; set; }
        public DateTimeOffset FirstRun { get; set; }
        public double MessagePanelHeight { get; set; }
        public bool EnableStatusLogging { get; set; }
        public bool CheckForUpdates { get; set; }
        public string GitHash { get; set; }
    }
}
