using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Settings
{
    class GeneralSettings
    {
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public string ContactListSortField { get; set; }
        public bool SpellCheck { get; set; }
        public string DownloadsFolder { get; set; }

        public GeneralSettings()
        {
            HideToSystemTray = true;
            ShowPopups = true;
            ContactListSortField = "DisplayName";
            SpellCheck = true;
            DownloadsFolder = "Downloads";
        }
    }
}
