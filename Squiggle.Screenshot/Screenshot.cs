using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Squiggle.Screenshot
{
    class Screenshot
    {
        // P/Invoke declarations
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

        public static Stream Capture()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            IntPtr hDesktopWnd = GetDesktopWindow();
            IntPtr hDesktopDC = GetWindowDC(hDesktopWnd);
            IntPtr hBitmapDC = CreateCompatibleDC(hDesktopDC);
            IntPtr hBitmap = CreateCompatibleBitmap(hDesktopDC, screenSize.Width, screenSize.Height);
            IntPtr hOldBitmap = SelectObject(hBitmapDC, hBitmap);

            BitBlt(hBitmapDC, 0, 0, screenSize.Width, screenSize.Height, hDesktopDC, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            
            using (Bitmap bmp = Bitmap.FromHbitmap(hBitmap))
            {
                SelectObject(hBitmapDC, hOldBitmap);
                DeleteObject(hBitmap);
                DeleteDC(hBitmapDC);
                ReleaseDC(hDesktopWnd, hDesktopDC);

                var stream = new MemoryStream();
                bmp.Save(stream, ImageFormat.Jpeg);
                return stream;
            }
        }        
    }
}
