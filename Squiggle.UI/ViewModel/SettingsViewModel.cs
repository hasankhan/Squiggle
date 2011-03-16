using System.Collections.Generic;
using Squiggle.UI.Settings;

namespace Squiggle.UI.ViewModel
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

            GeneralSettings.ShowEmoticons = settings.GeneralSettings.ShowEmoticons;
            GeneralSettings.HideToSystemTray = settings.GeneralSettings.HideToSystemTray;
            GeneralSettings.ShowPopups = settings.GeneralSettings.ShowPopups;
            GeneralSettings.AudioAlerts = settings.GeneralSettings.AudioAlerts;
            GeneralSettings.SpellCheck = settings.GeneralSettings.SpellCheck;
            GeneralSettings.EnableLogging = settings.GeneralSettings.EnableLogging;
            GeneralSettings.ContactListSortField = settings.GeneralSettings.ContactListSortField;
            GeneralSettings.GroupContacts = settings.GeneralSettings.GroupContacts;
            GeneralSettings.DownloadsFolder = settings.GeneralSettings.DownloadsFolder;

            ConnectionSettings.BindToIP = settings.ConnectionSettings.BindToIP;
            ConnectionSettings.ChatPort = settings.ConnectionSettings.ChatPort;
            ConnectionSettings.KeepAliveTime = settings.ConnectionSettings.KeepAliveTime;
            ConnectionSettings.PresencePort = settings.ConnectionSettings.PresencePort;

            PersonalSettings.DisplayMessage = settings.PersonalSettings.DisplayMessage;
            PersonalSettings.DisplayName = settings.PersonalSettings.DisplayName;
            PersonalSettings.GroupName = settings.PersonalSettings.GroupName;
            PersonalSettings.RememberMe = settings.PersonalSettings.RememberMe;
            PersonalSettings.AutoSignMeIn = settings.PersonalSettings.AutoSignMeIn;
            PersonalSettings.IdleTimeout = settings.PersonalSettings.IdleTimeout;
        }

        public void Update()
        {
            if (settings == null)
                return;

            settings.GeneralSettings.ShowEmoticons = GeneralSettings.ShowEmoticons;
            settings.GeneralSettings.HideToSystemTray = GeneralSettings.HideToSystemTray;
            settings.GeneralSettings.ShowPopups = GeneralSettings.ShowPopups;
            settings.GeneralSettings.AudioAlerts = GeneralSettings.AudioAlerts;
            settings.GeneralSettings.SpellCheck = GeneralSettings.SpellCheck;
            settings.GeneralSettings.EnableLogging = GeneralSettings.EnableLogging;
            settings.GeneralSettings.GroupContacts = GeneralSettings.GroupContacts;
            settings.GeneralSettings.ContactListSortField = GeneralSettings.ContactListSortField;
            settings.GeneralSettings.DownloadsFolder = GeneralSettings.DownloadsFolder;

            settings.ConnectionSettings.BindToIP = ConnectionSettings.BindToIP;
            settings.ConnectionSettings.ChatPort = ConnectionSettings.ChatPort;
            settings.ConnectionSettings.KeepAliveTime = ConnectionSettings.KeepAliveTime;
            settings.ConnectionSettings.PresencePort = ConnectionSettings.PresencePort;

            settings.PersonalSettings.DisplayMessage = PersonalSettings.DisplayMessage;
            settings.PersonalSettings.GroupName = PersonalSettings.GroupName;
            settings.PersonalSettings.DisplayName = PersonalSettings.DisplayName;
            settings.PersonalSettings.IdleTimeout = PersonalSettings.IdleTimeout;
            settings.PersonalSettings.RememberMe = PersonalSettings.RememberMe;
            settings.PersonalSettings.AutoSignMeIn = PersonalSettings.AutoSignMeIn;
        }
    }

    class PersonalSettingsViewModel
    {
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public string DisplayMessage { get; set; }
        public int IdleTimeout { get; set; }
        public bool RememberMe { get; set; }
        public bool AutoSignMeIn { get; set; }
    }

    class GeneralSettingsViewModel
    {
        public bool ShowEmoticons { get; set; }
        public bool RunAtStartup { get; set; }
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public string ContactListSortField { get; set; }
        public bool GroupContacts { get; set; }
        public bool SpellCheck { get; set; }
        public string DownloadsFolder { get; set; }
        public bool AudioAlerts { get; set; }
        public bool EnableLogging { get; set; }
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
