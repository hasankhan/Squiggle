using Microsoft.Win32;

namespace Squiggle.Utilities.Application
{
    public class WinStartup
    {
        public static bool IsAdded(string key, string path)
        {
            using (var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                bool added = path.Equals(runKey.GetValue(key));
                return added;
            }
        }

        public static void Add(string key, string path)
        {
            using (var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                runKey.SetValue(key, path);
        }

        public static void Remove(string key)
        {
            using (var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                runKey.DeleteValue(key, false);
        }
    }
}
