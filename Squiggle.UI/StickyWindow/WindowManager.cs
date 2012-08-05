using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Squiggle.UI.StickyWindow
{
    public static class WindowManager
    {
        static List<Window> widows;
        
        static WindowManager()
        {
            widows = new List<Window>();            
        }

        public static List<Window> Windows
        {
            get { return widows; }
        }

        public static void RegisterWindow(Window window)
        {
            widows.Add(window);
        }

        public static void UnregisterWindow(Window window)
        {
            widows.Remove(window);
        }
    }
}
