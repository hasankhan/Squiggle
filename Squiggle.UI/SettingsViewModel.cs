using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;

namespace Squiggle.UI
{
    class SettingsViewModel
    {
        public GeneralSettingsViewModel GeneralSettings { get; set; }
        public ConnectionSettingsViewModel ConnectionSettings { get; set; }
        public PersonalSettingsViewModel PersonalSettings { get; set; }

        public SettingsViewModel()
        {
            GeneralSettings = new GeneralSettingsViewModel();
            ConnectionSettings = new ConnectionSettingsViewModel();
            PersonalSettings = new PersonalSettingsViewModel();
        }
    }

    class PersonalSettingsViewModel
    {
        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public int IdleTimeout { get; set; }        
    }

    class GeneralSettingsViewModel
    {
        public bool RunAtStartup { get; set; }
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
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
