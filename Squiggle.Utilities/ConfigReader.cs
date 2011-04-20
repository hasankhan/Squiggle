using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Squiggle.Utilities
{
    public static class ConfigReader
    {
        public static T GetSetting<T>(string name)
        {
            return GetSetting<T>(name, default(T));
        }

        public static T GetSetting<T>(string name, T fallbackValue)
        {
            return GetSetting<T>(name, () => fallbackValue);
        }

        public static T GetSetting<T>(string name, Func<T> fallbackValueProvider)
        {
            string setting = ConfigurationManager.AppSettings[name];
            if (String.IsNullOrEmpty(setting))
                return fallbackValueProvider();

            try
            {
                var result = (T)Convert.ChangeType(setting, typeof(T));
                return result;
            }
            catch (InvalidCastException)
            {
                return fallbackValueProvider();
            }
        }
    }
}
