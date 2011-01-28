using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;

namespace Squiggle.UI.Resources
{
    static class Translation
    {
        public static string Global_ContactSays { get; private set; }
        public static string Global_You { get; private set; }
        public static string Global_ContactSaid { get; private set; }
        public static string Popup_NewMessage { get; private set; }

        static Translation()
        {
            foreach (PropertyInfo property in typeof(Translation).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var translation = Application.Current.FindResource(property.Name) as String;
                property.SetValue(null, translation, null);
            }
        }
    }
}
