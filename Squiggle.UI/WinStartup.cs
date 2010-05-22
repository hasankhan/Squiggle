using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Squiggle.UI
{
    class WinStartup
    {
        public static bool IsAdded(string key)
        {
            using (var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                bool added = runKey.GetValue(key) != null;
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
