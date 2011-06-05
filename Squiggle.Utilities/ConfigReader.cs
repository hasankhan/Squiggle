using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Squiggle.Utilities
{
    public class ConfigReader
    {
        Configuration config;

        public ConfigReader()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public void SetSetting<T>(string name, T value)
        {
            string strValue = Cast<string>(value);
            config.AppSettings.Settings.Remove(name);
            config.AppSettings.Settings.Add(name, strValue);
        }

        public T GetSetting<T>(string name)
        {
            return GetSetting<T>(name, default(T));
        }

        public T GetSetting<T>(string name, T fallbackValue)
        {
            return GetSetting<T>(name, () => fallbackValue);
        }

        public T GetSetting<T>(string name, Func<T> fallbackValueProvider)
        {
            KeyValueConfigurationElement setting = config.AppSettings.Settings[name];
            if (setting == null || String.IsNullOrEmpty(setting.Value))
                return fallbackValueProvider();

            try
            {
                return Cast<T>(setting.Value);
            }
            catch (InvalidCastException)
            {
                return fallbackValueProvider();
            }
        }

        public void Save()
        {
            config.Save(ConfigurationSaveMode.Full);
        }

        static T Cast<T>(object value)
        {
            var result = (T)Convert.ChangeType(value, typeof(T));
            return result;
        }
    }
}
