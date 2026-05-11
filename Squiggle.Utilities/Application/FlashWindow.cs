using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Squiggle.Utilities.Application
{
    [SupportedOSPlatform("windows")]
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

        public const UInt32 FLASHW_STOP = 0;
        public const UInt32 FLASHW_CAPTION = 1;
        public const UInt32 FLASHW_TRAY = 2;
        public const UInt32 FLASHW_ALL = 3;
        public const UInt32 FLASHW_TIMER = 4;
        public const UInt32 FLASHW_TIMERNOFG = 12;

        IntPtr handle;

        public FlashWindow(IntPtr windowHandle)
        {
            handle = windowHandle;
        }

        public bool Start()
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = handle;
            fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            fInfo.uCount = 3;
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }
    }
}
