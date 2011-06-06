
using System.Collections.Specialized;
using System;
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

        public GeneralSettings()
        {
            MessagePanelHeight = 70;
            HideToSystemTray = true;
            ShowPopups = true;
            AudioAlerts = false;
        }
    }
}
