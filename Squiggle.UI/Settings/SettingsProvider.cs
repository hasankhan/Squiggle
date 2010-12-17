using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Squiggle.Chat;
using Squiggle.UI.Helpers;

namespace Squiggle.UI.Settings
{
    class SettingsProvider
    {
        public static SettingsProvider Current { get; private set; }

        static SettingsProvider()
        {
            Current = new SettingsProvider();
        }

        public event EventHandler SettingsUpdated = delegate { };

        public SettingsProvider()
        {
            Load();
        }

        public SquiggleSettings Settings { get; private set; }

        public void Load()
        {
            if (Settings == null)
                Settings = new SquiggleSettings();
            LoadGeneralSettings();
            LoadPersonalSettings();
            LoadConnectionSettings();
        }

        public void Save()
        {
            SaveGeneralSettings();
            SavePersonalSettings();
            SaveConnectionSettings();

            Properties.Settings.Default.Save();

            SettingsUpdated(this, EventArgs.Empty);
        }

        public void Update(Action<SquiggleSettings> updateAction)
        {
            updateAction(Settings);
            Save();
        }

        private void LoadConnectionSettings()
        {
            Settings.ConnectionSettings.PresenceAddress = Properties.Settings.Default.PresenceAddress;
            Settings.ConnectionSettings.BindToIP = Properties.Settings.Default.BindToIP;
            bool requiresBindIP = String.IsNullOrEmpty(Settings.ConnectionSettings.BindToIP);
            if (!requiresBindIP)
            {
                var ip = IPAddress.Parse(Settings.ConnectionSettings.BindToIP);
                requiresBindIP = !NetworkUtility.IsValidIP(ip);
            }
            if (requiresBindIP)
            {
                var ip = NetworkUtility.GetLocalIPAddress();
                if (ip != null)
                    Settings.ConnectionSettings.BindToIP = ip.ToString();
            }
            Settings.ConnectionSettings.ChatPort = Properties.Settings.Default.ChatPort;
            Settings.ConnectionSettings.KeepAliveTime = Properties.Settings.Default.KeepAliveTime;
            Settings.ConnectionSettings.PresencePort = Properties.Settings.Default.PresencePort;
        }

        private void LoadGeneralSettings()
        {
            Settings.GeneralSettings.ShowEmoticons = Properties.Settings.Default.ShowEmoticons;
            Settings.GeneralSettings.HideToSystemTray = Properties.Settings.Default.HideToTray;
            Settings.GeneralSettings.ShowPopups = Properties.Settings.Default.ShowPopups;
            Settings.GeneralSettings.SpellCheck = Properties.Settings.Default.SpellCheck;
            Settings.GeneralSettings.ContactListSortField = Properties.Settings.Default.ContactListSortField;
            Settings.GeneralSettings.GroupContacts = Properties.Settings.Default.GroupContacts;
            Settings.GeneralSettings.ContactGroups = Properties.Settings.Default.Groups ?? new ContactGroups();
            if (String.IsNullOrEmpty(Properties.Settings.Default.DownloadsFolder) || !Shell.CreateDirectoryIfNotExists(Properties.Settings.Default.DownloadsFolder))
            {
                string location = SquiggleUtility.GetInstallDirectory();
                Settings.GeneralSettings.DownloadsFolder = Path.Combine(location, "Downloads");
            }
            else
                Settings.GeneralSettings.DownloadsFolder = Properties.Settings.Default.DownloadsFolder;
        }

        private void LoadPersonalSettings()
        {
            Settings.PersonalSettings.RememberMe = !String.IsNullOrEmpty(Properties.Settings.Default.DisplayName);
            Settings.PersonalSettings.DisplayName = Properties.Settings.Default.DisplayName;
            Settings.PersonalSettings.DisplayMessage = Properties.Settings.Default.DisplayMessage;
            Settings.PersonalSettings.GroupName = Properties.Settings.Default.GroupName;
            Settings.PersonalSettings.AutoSignMeIn = Properties.Settings.Default.AutoSignIn;
            Settings.PersonalSettings.IdleTimeout = Properties.Settings.Default.IdleTimeout;
            Settings.PersonalSettings.FontColor = Properties.Settings.Default.FontColor;
            Settings.PersonalSettings.FontStyle = Properties.Settings.Default.FontStyle;
            Settings.PersonalSettings.FontSize = Properties.Settings.Default.FontSize;
            Settings.PersonalSettings.FontName = Properties.Settings.Default.FontName;

        }

        private void SavePersonalSettings()
        {
            Properties.Settings.Default.DisplayName = Settings.PersonalSettings.RememberMe ? Settings.PersonalSettings.DisplayName : String.Empty;
            Properties.Settings.Default.DisplayMessage = Settings.PersonalSettings.RememberMe ? Settings.PersonalSettings.DisplayMessage : String.Empty;
            Properties.Settings.Default.GroupName = Settings.PersonalSettings.RememberMe ? Settings.PersonalSettings.GroupName : String.Empty;

            Properties.Settings.Default.AutoSignIn = Settings.PersonalSettings.AutoSignMeIn;
            Properties.Settings.Default.IdleTimeout = Settings.PersonalSettings.IdleTimeout;
            Properties.Settings.Default.FontColor = Settings.PersonalSettings.FontColor;
            Properties.Settings.Default.FontStyle = Settings.PersonalSettings.FontStyle;
            Properties.Settings.Default.FontSize = Settings.PersonalSettings.FontSize;
            Properties.Settings.Default.FontName = Settings.PersonalSettings.FontName;
        }

        private void SaveConnectionSettings()
        {
            Properties.Settings.Default.PresenceAddress = Settings.ConnectionSettings.PresenceAddress;
            Properties.Settings.Default.BindToIP = Settings.ConnectionSettings.BindToIP;
            Properties.Settings.Default.ChatPort = Settings.ConnectionSettings.ChatPort;
            Properties.Settings.Default.KeepAliveTime = Settings.ConnectionSettings.KeepAliveTime;
            Properties.Settings.Default.PresencePort = Settings.ConnectionSettings.PresencePort;
        }

        private void SaveGeneralSettings()
        {
            Properties.Settings.Default.ShowEmoticons = Settings.GeneralSettings.ShowEmoticons;
            Properties.Settings.Default.DownloadsFolder = Settings.GeneralSettings.DownloadsFolder;
            Properties.Settings.Default.HideToTray = Settings.GeneralSettings.HideToSystemTray;
            Properties.Settings.Default.ShowPopups = Settings.GeneralSettings.ShowPopups;
            Properties.Settings.Default.ContactListSortField = Settings.GeneralSettings.ContactListSortField;
            Properties.Settings.Default.SpellCheck = Settings.GeneralSettings.SpellCheck;
            Properties.Settings.Default.DownloadsFolder = Settings.GeneralSettings.DownloadsFolder;
            Properties.Settings.Default.GroupContacts = Settings.GeneralSettings.GroupContacts;
            Properties.Settings.Default.Groups = Settings.GeneralSettings.ContactGroups;
        }
    }
}
