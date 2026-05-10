using Squiggle.UI.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Squiggle.Utilities;

namespace Squiggle.UI.Components
{
    class AppInfo
    {
        public static string Location { get; private set; } = null!;
        public static string FilePath { get; private set; } = null!;
        public static Version Version { get; private set; } = null!;
        public static string Hash { get; private set; } = null!;

        static AppInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            FilePath = assembly.Location;
            Location = Path.GetDirectoryName(FilePath);
            Version = assembly.GetName().Version;

            Hash = SettingsProvider.Current.Coalesce(provider => provider.Settings.GeneralSettings.GitHash, String.Empty);
        }
    }
}
