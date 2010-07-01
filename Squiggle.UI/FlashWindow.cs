using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Squiggle.UI
{
    public class FlashWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        //Stop flashing. The system restores the window to its original state.
        public const UInt32 FLASHW_STOP = 0;
        //Flash the window caption.
        public const UInt32 FLASHW_CAPTION = 1;
        //Flash the taskbar button.
        public const UInt32 FLASHW_TRAY = 2;
        //Flash both the window caption and taskbar button.
        //This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        public const UInt32 FLASHW_ALL = 3;
        //Flash continuously, until the FLASHW_STOP flag is set.
        public const UInt32 FLASHW_TIMER = 4;
        //Flash continuously until the window comes to the foreground.
        public const UInt32 FLASHW_TIMERNOFG = 12; 

        private Window target;

        public FlashWindow(Window sender)
        {
            target = (Window)sender;
        }

        public bool Start()
        {
            IntPtr hWnd = new WindowInteropHelper(target).Handle;
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            fInfo.uCount = 3;
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }
    }
}
