using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;
using Squiggle.UI.Settings;

namespace Squiggle.UI
{
    class SettingsViewModel
    {
        SquiggleSettings settings;

        public GeneralSettingsViewModel GeneralSettings { get; set; }
        public ConnectionSettingsViewModel ConnectionSettings { get; set; }
        public PersonalSettingsViewModel PersonalSettings { get; set; }

        public SettingsViewModel()
        {
            GeneralSettings = new GeneralSettingsViewModel();
            ConnectionSettings = new ConnectionSettingsViewModel();
            PersonalSettings = new PersonalSettingsViewModel();
        }

        public SettingsViewModel(SquiggleSettings settings): this()
        {
            this.settings = settings;

            GeneralSettings.HideToSystemTray = settings.GeneralSettings.HideToSystemTray;
            GeneralSettings.ShowPopups = settings.GeneralSettings.ShowPopups;
            GeneralSettings.SpellCheck = settings.GeneralSettings.SpellCheck;

            ConnectionSettings.BindToIP = settings.ConnectionSettings.BindToIP;
            ConnectionSettings.ChatPort = settings.ConnectionSettings.ChatPort;
            ConnectionSettings.KeepAliveTime = settings.ConnectionSettings.KeepAliveTime;
            ConnectionSettings.PresencePort = settings.ConnectionSettings.PresencePort;

            PersonalSettings.DisplayMessage = settings.PersonalSettings.DisplayMessage;
            PersonalSettings.DisplayName = settings.PersonalSettings.DisplayName;
            PersonalSettings.RememberMe = settings.PersonalSettings.RememberMe;
            PersonalSettings.AutoSignMeIn = settings.PersonalSettings.AutoSignMeIn;
            PersonalSettings.IdleTimeout = settings.PersonalSettings.IdleTimeout;
        }

        public void Update()
        {
            if (settings == null)
                return;

            settings.GeneralSettings.HideToSystemTray = GeneralSettings.HideToSystemTray;
            settings.GeneralSettings.ShowPopups = GeneralSettings.ShowPopups;
            settings.GeneralSettings.SpellCheck = GeneralSettings.SpellCheck;

            settings.ConnectionSettings.BindToIP = ConnectionSettings.BindToIP;
            settings.ConnectionSettings.ChatPort = ConnectionSettings.ChatPort;
            settings.ConnectionSettings.KeepAliveTime = ConnectionSettings.KeepAliveTime;
            settings.ConnectionSettings.PresencePort = ConnectionSettings.PresencePort;

            settings.PersonalSettings.DisplayMessage = PersonalSettings.DisplayMessage;
            settings.PersonalSettings.DisplayName = PersonalSettings.DisplayName;
            settings.PersonalSettings.IdleTimeout = PersonalSettings.IdleTimeout;
            settings.PersonalSettings.RememberMe = PersonalSettings.RememberMe;
            settings.PersonalSettings.AutoSignMeIn = PersonalSettings.AutoSignMeIn;
        }
    }

    class PersonalSettingsViewModel
    {
        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public int IdleTimeout { get; set; }
        public bool RememberMe { get; set; }
        public bool AutoSignMeIn { get; set; }
    }

    class GeneralSettingsViewModel
    {
        public bool RunAtStartup { get; set; }
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public bool SpellCheck { get; set; }
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
