using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Squiggle.Chat;
using Squiggle.Core;
using Squiggle.UI.Helpers;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.Utilities.Net;
using Squiggle.UI.Components;

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
            var reader = new ConfigReader();

            LoadGeneralSettings(reader);
            LoadPersonalSettings(reader);
            LoadChatSettings(reader);
            LoadContactSettings();
            LoadConnectionSettings(reader);
        }

        public void Save()
        {
            var reader = new ConfigReader();
            SaveGeneralSettings(reader);
            SavePersonalSettings(reader);
            SaveChatSettings(reader);
            SaveContactSettings();
            SaveConnectionSettings(reader);

            ExceptionMonster.EatTheException(() =>
            {
                Properties.Settings.Default.Save();
                reader.Save();
            }, "saving configuration settings");

            SettingsUpdated(this, EventArgs.Empty);
        }

        public void Update(Action<SquiggleSettings> updateAction)
        {
            updateAction(Settings);
            Save();
        }

        private void LoadConnectionSettings(ConfigReader reader)
        {
            Settings.ConnectionSettings.PresenceAddress = reader.GetSetting(SettingKey.PresenceAddress, DefaultValues.PresenceAddress);
            Settings.ConnectionSettings.BindToIP = Properties.Settings.Default.BindToIP;

            bool requiresNewBindToIP = !NetworkUtility.IsValidLocalIP(Settings.ConnectionSettings.BindToIP);
            if (requiresNewBindToIP)
            {
                var ip = NetworkUtility.GetLocalIPAddress();
                Settings.ConnectionSettings.BindToIP = ip == null ? String.Empty : ip.ToString();
            }

            Settings.ConnectionSettings.ClientID = Properties.Settings.Default.ClientID;

            if (String.IsNullOrEmpty(Settings.ConnectionSettings.ClientID))
                Settings.ConnectionSettings.ClientID = Guid.NewGuid().ToString();

            Settings.ConnectionSettings.ChatPort = reader.GetSetting(SettingKey.ChatPort, DefaultValues.ChatPort);
            Settings.ConnectionSettings.KeepAliveTime = reader.GetSetting(SettingKey.KeepAliveTime, DefaultValues.KeepAliveTime);
            Settings.ConnectionSettings.PresencePort = reader.GetSetting(SettingKey.PresencePort, DefaultValues.PresencePort);
            Settings.ConnectionSettings.PresenceCallbackPort = reader.GetSetting(SettingKey.PresenceCallbackPort, DefaultValues.PresenceCallbackPort);
        }

        private void LoadGeneralSettings(ConfigReader reader)
        {
            DateTimeOffset firstRun;
            if (!DateTimeOffset.TryParse(Properties.Settings.Default.FirstRun, out firstRun))
                firstRun = DateTimeOffset.Now;

            Settings.GeneralSettings.FirstRun = firstRun;
            Settings.GeneralSettings.MessagePanelHeight = Math.Max(Properties.Settings.Default.MessagePanelHeight, 150);
            Settings.GeneralSettings.HideToSystemTray = Properties.Settings.Default.HideToTray;
            Settings.GeneralSettings.ShowPopups = Properties.Settings.Default.ShowPopups;
            Settings.GeneralSettings.AudioAlerts = Properties.Settings.Default.AudioAlerts;
            Settings.GeneralSettings.EnableStatusLogging = reader.GetSetting(SettingKey.EnableStatusLogging, false);
            Settings.GeneralSettings.CheckForUpdates = reader.GetSetting(SettingKey.CheckForUpdates, true);

            if (String.IsNullOrEmpty(Properties.Settings.Default.DownloadsFolder) || !Shell.CreateDirectoryIfNotExists(Properties.Settings.Default.DownloadsFolder))
                Settings.GeneralSettings.DownloadsFolder = Path.Combine(AppInfo.Location, "Downloads");
            else
                Settings.GeneralSettings.DownloadsFolder = Properties.Settings.Default.DownloadsFolder;
        }

        private void LoadPersonalSettings(ConfigReader reader)
        {
            Settings.PersonalSettings.RememberMe = !String.IsNullOrEmpty(Properties.Settings.Default.DisplayName);
            Settings.PersonalSettings.DisplayName = Properties.Settings.Default.DisplayName;
            Settings.PersonalSettings.DisplayMessage = Properties.Settings.Default.DisplayMessage;
            Settings.PersonalSettings.DisplayImage = Properties.Settings.Default.DisplayImage;
            Settings.PersonalSettings.EmailAddress = Properties.Settings.Default.EmailAddress;
            Settings.PersonalSettings.GroupName = Properties.Settings.Default.GroupName;
            Settings.PersonalSettings.AutoSignMeIn = reader.GetSetting(SettingKey.AutoSignIn, false);
            Settings.PersonalSettings.IdleTimeout = reader.GetSetting(SettingKey.IdleTimeout, 5);
            Settings.PersonalSettings.FontColor = Properties.Settings.Default.FontColor;
            Settings.PersonalSettings.FontStyle = Properties.Settings.Default.FontStyle;
            Settings.PersonalSettings.FontSize = Properties.Settings.Default.FontSize;
            Settings.PersonalSettings.FontName = Properties.Settings.Default.FontName;
        }

        void LoadChatSettings(ConfigReader reader)
        {
            Settings.ChatSettings.ShowEmoticons = Properties.Settings.Default.ShowEmoticons;
            Settings.ChatSettings.SpellCheck = Properties.Settings.Default.SpellCheck;
            Settings.ChatSettings.StealFocusOnNewMessage = Properties.Settings.Default.StealFocusOnNewMessage;
            Settings.ChatSettings.ClearChatOnWindowClose = Properties.Settings.Default.ClearChatOnWindowClose;
            Settings.ChatSettings.EnableLogging = reader.GetSetting(SettingKey.EnableChatLogging, false);
            Settings.ChatSettings.MaxMessagesToPreserve = reader.GetSetting(SettingKey.MaxMessagesToPreserve, 100);
        }

        void LoadContactSettings()
        {
            Settings.ContactSettings.ContactListSortField = Properties.Settings.Default.ContactListSortField;
            Settings.ContactSettings.GroupContacts = Properties.Settings.Default.GroupContacts;
            Settings.ContactSettings.ContactGroups = Properties.Settings.Default.Groups ?? new ContactGroups();
            Settings.ContactSettings.ShowOfflineContatcs = Properties.Settings.Default.ShowOfflineContatcs;
            Settings.ContactSettings.ContactListView = Properties.Settings.Default.ContactListView;
        }

        private void SavePersonalSettings(ConfigReader reader)
        {
            Properties.Settings.Default.DisplayName = Settings.PersonalSettings.RememberMe ? Settings.PersonalSettings.DisplayName : String.Empty;
            Properties.Settings.Default.DisplayMessage = Settings.PersonalSettings.RememberMe ? Settings.PersonalSettings.DisplayMessage : String.Empty;
            Properties.Settings.Default.DisplayImage = Settings.PersonalSettings.RememberMe ? Settings.PersonalSettings.DisplayImage : null;
            Properties.Settings.Default.GroupName = Settings.PersonalSettings.RememberMe ? Settings.PersonalSettings.GroupName : String.Empty;
            Properties.Settings.Default.EmailAddress = Settings.PersonalSettings.RememberMe ? Settings.PersonalSettings.EmailAddress : String.Empty;

            reader.SetSetting(SettingKey.AutoSignIn, Settings.PersonalSettings.AutoSignMeIn);
            reader.SetSetting(SettingKey.IdleTimeout, Settings.PersonalSettings.IdleTimeout);
            Properties.Settings.Default.FontColor = Settings.PersonalSettings.FontColor;
            Properties.Settings.Default.FontStyle = Settings.PersonalSettings.FontStyle;
            Properties.Settings.Default.FontSize = Settings.PersonalSettings.FontSize;
            Properties.Settings.Default.FontName = Settings.PersonalSettings.FontName;
        }

        private void SaveConnectionSettings(ConfigReader reader)
        {
            reader.SetSetting(SettingKey.PresenceAddress, Settings.ConnectionSettings.PresenceAddress);
            Properties.Settings.Default.BindToIP = Settings.ConnectionSettings.BindToIP;
            reader.SetSetting(SettingKey.ChatPort, Settings.ConnectionSettings.ChatPort);
            reader.SetSetting(SettingKey.KeepAliveTime, Settings.ConnectionSettings.KeepAliveTime);
            reader.SetSetting(SettingKey.PresencePort, Settings.ConnectionSettings.PresencePort);
            reader.SetSetting(SettingKey.PresenceCallbackPort, Settings.ConnectionSettings.PresenceCallbackPort);
            Properties.Settings.Default.ClientID = Settings.ConnectionSettings.ClientID;
        }

        private void SaveGeneralSettings(ConfigReader reader)
        {
            Properties.Settings.Default.MessagePanelHeight = Settings.GeneralSettings.MessagePanelHeight;
            Properties.Settings.Default.FirstRun = Settings.GeneralSettings.FirstRun.ToString();
            Properties.Settings.Default.DownloadsFolder = Settings.GeneralSettings.DownloadsFolder;
            Properties.Settings.Default.HideToTray = Settings.GeneralSettings.HideToSystemTray;
            Properties.Settings.Default.ShowPopups = Settings.GeneralSettings.ShowPopups;
            Properties.Settings.Default.AudioAlerts = Settings.GeneralSettings.AudioAlerts;
            Properties.Settings.Default.DownloadsFolder = Settings.GeneralSettings.DownloadsFolder;
            reader.SetSetting(SettingKey.EnableStatusLogging, Settings.GeneralSettings.EnableStatusLogging);
            reader.SetSetting(SettingKey.CheckForUpdates, Settings.GeneralSettings.CheckForUpdates);
        }

        void SaveChatSettings(ConfigReader reader)
        {
            Properties.Settings.Default.ShowEmoticons = Settings.ChatSettings.ShowEmoticons;
            Properties.Settings.Default.SpellCheck = Settings.ChatSettings.SpellCheck;
            Properties.Settings.Default.StealFocusOnNewMessage = Settings.ChatSettings.StealFocusOnNewMessage;
            Properties.Settings.Default.ClearChatOnWindowClose = Settings.ChatSettings.ClearChatOnWindowClose;
            reader.SetSetting(SettingKey.EnableChatLogging, Settings.ChatSettings.EnableLogging);
        }

        void SaveContactSettings()
        {
            Properties.Settings.Default.ContactListSortField = Settings.ContactSettings.ContactListSortField;
            Properties.Settings.Default.GroupContacts = Settings.ContactSettings.GroupContacts;
            Properties.Settings.Default.Groups = Settings.ContactSettings.ContactGroups;
            Properties.Settings.Default.ShowOfflineContatcs = Settings.ContactSettings.ShowOfflineContatcs;
            Properties.Settings.Default.ContactListView = Settings.ContactSettings.ContactListView;
        }
    }
}
