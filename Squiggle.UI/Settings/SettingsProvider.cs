using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using System;

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
            ConfigReader reader = GetConfigReader();

            Settings.ReadFrom(Properties.Settings.Default, reader);
        }        

        public void Save()
        {
            ConfigReader reader = GetConfigReader();

            Settings.WriteTo(Properties.Settings.Default, reader);

            ExceptionMonster.EatTheException(() =>
            {
                Properties.Settings.Default.Save();
                reader.Save();
            }, "saving configuration settings");

            SettingsUpdated(this, EventArgs.Empty);
        }

        static ConfigReader GetConfigReader()
        {
            var reader = new ConfigReader();
            reader.ReadOnly = reader.GetSetting(SettingKey.ReadOnly, false);
            return reader;
        }
    }
}
