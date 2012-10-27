using Squiggle.UI.Components;
using Squiggle.Utilities.Application;
using System;
using System.Collections.Specialized;
using System.IO;

namespace Squiggle.UI.Settings
{
    class GeneralSettings : ISettingsGroup
    {
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public bool AudioAlerts { get; set; }
        public string DownloadsFolder { get; set; }
        public DateTimeOffset FirstRun { get; set; }
        public double MessagePanelHeight { get; set; }
        public bool EnableStatusLogging { get; set; }
        public bool CheckForUpdates { get; set; }
        public string GitHash { get; set; }

        public void ReadFrom(Properties.Settings settings, ConfigReader reader)
        {
            DateTimeOffset firstRun;
            if (!DateTimeOffset.TryParse(Properties.Settings.Default.FirstRun, out firstRun))
                firstRun = DateTimeOffset.Now;

            FirstRun = firstRun;
            MessagePanelHeight = Math.Max(settings.MessagePanelHeight, 150);
            HideToSystemTray = settings.HideToTray;
            ShowPopups = settings.ShowPopups;
            AudioAlerts = settings.AudioAlerts;
            EnableStatusLogging = reader.GetSetting(SettingKey.EnableStatusLogging, false);
            CheckForUpdates = reader.GetSetting(SettingKey.CheckForUpdates, true);
            GitHash = reader.GetSetting(SettingKey.GitHash, String.Empty);

            if (String.IsNullOrEmpty(settings.DownloadsFolder) || !Shell.CreateDirectoryIfNotExists(settings.DownloadsFolder))
                DownloadsFolder = Path.Combine(AppInfo.Location, "Downloads");
            else
                DownloadsFolder = settings.DownloadsFolder;
        }

        public void WriteTo(Properties.Settings settings, ConfigReader reader)
        {
            settings.MessagePanelHeight = MessagePanelHeight;
            settings.FirstRun = FirstRun.ToString();
            settings.DownloadsFolder = DownloadsFolder;
            settings.HideToTray = HideToSystemTray;
            settings.ShowPopups = ShowPopups;
            settings.AudioAlerts = AudioAlerts;
            settings.DownloadsFolder = DownloadsFolder;
            reader.SetSetting(SettingKey.EnableStatusLogging, EnableStatusLogging);
            reader.SetSetting(SettingKey.CheckForUpdates, CheckForUpdates);
        }
    }
}
