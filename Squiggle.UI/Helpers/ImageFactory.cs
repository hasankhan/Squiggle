using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Squiggle.UI.Helpers
{
    public class ImageFactory
    {
        public static ImageFactory Instance = new ImageFactory();

        Dictionary<string, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();

        public BitmapImage Load(Uri source)
        {
            BitmapImage image;
            string key = source.ToString().ToUpperInvariant();
            if (!imageCache.TryGetValue(key, out image))
            {
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = source;
                image.EndInit();
                image.Freeze();

                imageCache[key] = image;
            }
                        
            return image;
        }
    }
}
