using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Squiggle.UI.Helpers
{
    static class ImageUtility
    {
        public static byte[] ToBytes(this Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static bool IsValidImage(string filename)
        {
            try
            {
                var newImage = new BitmapImage(new Uri(filename));
            }
            catch (NotSupportedException)
            {
                // System.NotSupportedException:
                // No imaging component suitable to complete this operation was found.
                return false;
            }
            return true;
        }

        public static Image ResizeTo(this Image image, int width, int height)
        {
            image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (image.Width <= width)
                width = image.Width;

            int newHeight = image.Height * width / image.Width;
            if (newHeight > height)
            {
                width = image.Width * height / image.Height;
                newHeight = height;
            }

            Image newImage = image.GetThumbnailImage(width, newHeight, null, IntPtr.Zero);

            return newImage;
        }
    }
}
