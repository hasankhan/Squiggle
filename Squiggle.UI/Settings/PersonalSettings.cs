using System.Drawing;

namespace Squiggle.UI.Settings
{
    class PersonalSettings
    {
        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public int IdleTimeout { get; set; }
        public bool RememberMe { get; set; }
        public bool AutoSignMeIn { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color FontColor { get; set; }
        public FontStyle FontStyle { get; set; }

        public PersonalSettings()
        {
            FontName = "Georgia";
            FontSize = 12;
            FontStyle = System.Drawing.FontStyle.Regular;
            FontColor = System.Drawing.Color.Black;
            IdleTimeout = 5;
        }

    }
}
