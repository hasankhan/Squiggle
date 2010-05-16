using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows;
using System.Timers;

namespace Squiggle.UI
{
    public class FlashForm
    {
        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        private Window target;

        public FlashForm(Window sender)
        {
            target = (Window)sender;
        }

        public void Start()
        {
            ToggleFlash(true);
        }

        public void Stop()
        {
            ToggleFlash(false);
        }

        void ToggleFlash(bool flash)
        {
            IntPtr hwnd = new WindowInteropHelper(target).Handle;
            FlashWindow(hwnd, flash);
        }
    }
}
