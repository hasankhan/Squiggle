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

        public FontSetting(System.Drawing.Color color, string familyName, float size, System.Drawing.FontStyle style)
        {
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            Family = new FontFamily(familyName);
            Size = size;
            Style = (style & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal; 
            Weight = (style & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;

            Font = new System.Drawing.Font(familyName, size, style);
        }
    }
}
