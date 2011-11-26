using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Squiggle.UI.Resources
{
    static class BrushFactory
    {
        static Dictionary<uint, SolidColorBrush> brushes = new Dictionary<uint, SolidColorBrush>();

        public static SolidColorBrush Create(Color color)
        {
            uint code = Convert.ToUInt32(color.ToString().Substring(1), 16);
            SolidColorBrush brush;                
            if (!brushes.TryGetValue(code, out brush))
            {
                brushes[code] = brush = new SolidColorBrush(color);
                brush.Freeze();
            }
            return brush;
        }
    }
}
