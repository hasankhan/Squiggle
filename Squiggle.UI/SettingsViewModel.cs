using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI
{
    class SettingsViewModel
    {
        public GeneralSettingsViewModel GeneralSettings { get; set; }
        public ConnectionSettingsViewModel ConnectionSettings { get; set; }

        public SettingsViewModel()
        {
            GeneralSettings = new GeneralSettingsViewModel();
            ConnectionSettings = new ConnectionSettingsViewModel();
        }
    }

    class GeneralSettingsViewModel
    {
        public bool RunAtStartup { get; set; }
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public int IdleTimeout { get; set; }
    }

    class ConnectionSettingsViewModel
    {
        public int PresencePort { get; set; }
        public int ChatPort { get; set; }
        public int KeepAliveTime { get; set; }
        public List<string> AllIPs { get; private set; }
        public string BindToIP { get; set; }

        public ConnectionSettingsViewModel()
        {
            AllIPs = new List<string>();
        }
    }
}
