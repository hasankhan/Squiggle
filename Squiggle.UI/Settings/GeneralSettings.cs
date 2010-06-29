using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

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
            DownloadsFolder = Path.Combine(Assembly.GetExecutingAssembly().Location, "Downloads");
        }
    }
}
