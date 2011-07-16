using System.Drawing;

namespace Squiggle.UI.Settings
{
    class PersonalSettings
    {
        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public byte[] DisplayImage { get; set; }
        public string GroupName { get; set; }
        public string EmailAddress { get; set; }
        public int IdleTimeout { get; set; }
        public bool RememberMe { get; set; }
        public bool AutoSignMeIn { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color FontColor { get; set; }
        public FontStyle FontStyle { get; set; }
    }
}
