using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;
using System.Net;
using System.Diagnostics;

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
            Settings.ConnectionSettings.BindToIP = Properties.Settings.Default.BindToIP;
            bool requiresBindIP = String.IsNullOrEmpty(Settings.ConnectionSettings.BindToIP);
            if (!requiresBindIP)
            {
                var ip = IPAddress.Parse(Settings.ConnectionSettings.BindToIP);
                requiresBindIP = !NetworkUtility.GetLocalIPAddresses().Contains(ip);
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
            Settings.GeneralSettings.HideToSystemTray = Properties.Settings.Default.HideToTray;
            Settings.GeneralSettings.ShowPopups = Properties.Settings.Default.ShowPopups;
            Settings.GeneralSettings.SpellCheck = Properties.Settings.Default.SpellCheck;
        }

        private void LoadPersonalSettings()
        {
            Settings.PersonalSettings.RememberMe = !String.IsNullOrEmpty(Properties.Settings.Default.DisplayName);
            Settings.PersonalSettings.DisplayName = Properties.Settings.Default.DisplayName;
            Settings.PersonalSettings.DisplayMessage = Properties.Settings.Default.DisplayMessage;
            Settings.PersonalSettings.AutoSignMeIn = Properties.Settings.Default.AutoSignIn;
            Settings.PersonalSettings.IdleTimeout = Properties.Settings.Default.IdleTimeout;
        }

        private void SavePersonalSettings()
        {
            Properties.Settings.Default.DisplayName = Settings.PersonalSettings.RememberMe ?
                                                                    Settings.PersonalSettings.DisplayName : String.Empty;

            Properties.Settings.Default.DisplayMessage = Settings.PersonalSettings.RememberMe ?
                                                                    Settings.PersonalSettings.DisplayMessage : String.Empty;

            Properties.Settings.Default.AutoSignIn = Settings.PersonalSettings.AutoSignMeIn;
            Properties.Settings.Default.IdleTimeout = Settings.PersonalSettings.IdleTimeout;
        }

        private void SaveConnectionSettings()
        {
            Properties.Settings.Default.BindToIP = Settings.ConnectionSettings.BindToIP;
            Properties.Settings.Default.ChatPort = Settings.ConnectionSettings.ChatPort;
            Properties.Settings.Default.KeepAliveTime = Settings.ConnectionSettings.KeepAliveTime;
            Properties.Settings.Default.PresencePort = Settings.ConnectionSettings.PresencePort;
        }

        private void SaveGeneralSettings()
        {
            Properties.Settings.Default.HideToTray = Settings.GeneralSettings.HideToSystemTray;
            Properties.Settings.Default.ShowPopups = Settings.GeneralSettings.ShowPopups;
            Properties.Settings.Default.SpellCheck = Settings.GeneralSettings.SpellCheck;
        }
    }
}
