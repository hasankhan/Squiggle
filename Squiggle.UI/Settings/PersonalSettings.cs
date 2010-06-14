using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Squiggle.UI.Settings
{
    class PersonalSettings
    {
        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public int IdleTimeout { get; set; }
        public bool RememberMe { get; set; }
        public bool AutoSignMeIn { get; set; }
        public FontFamily FontName { get; set; }
        public int FontSize { get; set; }
        public Color FontColor { get; set; }

        public PersonalSettings()
        {
            FontName = new FontFamily("Georgia");
            FontSize = 12;
            FontColor = Colors.Black;
            IdleTimeout = 5;
        }
    }
}
