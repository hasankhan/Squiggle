using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Font Font { get; set; }
        public int FontSize { get; set; }
        public Color FontColor { get; set; }
        public string FontWeight { get; set; }

        public PersonalSettings()
        {
            Font = new Font(new FontFamily("Georgia"), 12);
            FontSize = 12;
            FontColor = System.Drawing.Color.Black;
            FontWeight = "Regular";
            IdleTimeout = 5;
        }

    }
}
