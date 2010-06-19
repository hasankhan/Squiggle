using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Squiggle.UI
{
    public class FontSetting
    {
        public Brush Foreground { get; set; }
        public FontFamily Family { get; set; }
        public double Size { get; set; }
        public System.Windows.FontStyle Style { get; set; }
        public System.Windows.FontWeight Weight { get; set; }
        public System.Drawing.Font Font { get; private set; }

        public FontSetting(System.Drawing.Color color, string familyName, float size, string style, bool bold)
        {
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            Family = new FontFamily(familyName);
            Size = size;
            Style = style == "Italic" ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal; 
            Weight = bold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;

            if(!String.IsNullOrEmpty(style))
                Font = new System.Drawing.Font(familyName, size, GetStyle(style));
            else
                Font = new System.Drawing.Font(familyName, size);
        }

        private System.Drawing.FontStyle GetStyle(string style)
        {
            switch(style.ToLower())
            {
                case "italic":
                    return System.Drawing.FontStyle.Italic;

                case "bold":
                    return System.Drawing.FontStyle.Bold;

                default:
                    return System.Drawing.FontStyle.Regular;
            }
        }
    }
}
