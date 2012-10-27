using Squiggle.Utilities.Application;
using Squiggle.Utilities.Security;
using System;
using System.Drawing;

namespace Squiggle.UI.Settings
{
    class PersonalSettings : ISettingsGroup
    {
        public string DisplayName { get; set; }        
        public string GroupName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        public string DisplayMessage { get; set; }
        public byte[] DisplayImage { get; set; }
        public string EmailAddress { get; set; }

        public bool RememberMe { get; set; }
        public bool AutoSignMeIn { get; set; }

        public int IdleTimeout { get; set; }
       
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color FontColor { get; set; }
        public FontStyle FontStyle { get; set; }

        public void ReadFrom(Properties.Settings settings, ConfigReader reader)
        {
            RememberMe = !String.IsNullOrEmpty(settings.DisplayName);
            DisplayName = settings.DisplayName;
            DisplayMessage = settings.DisplayMessage;
            DisplayImage = settings.DisplayImage;
            EmailAddress = settings.EmailAddress;
            GroupName = settings.GroupName;
            Username = settings.Username;
            Password = ProtectedData.Unprotect(settings.Password);
            Domain = settings.Domain;

            AutoSignMeIn = reader.GetSetting(SettingKey.AutoSignIn, false);
            IdleTimeout = reader.GetSetting(SettingKey.IdleTimeout, 5);
            
            FontColor = settings.FontColor;
            FontStyle = settings.FontStyle;
            FontSize = settings.FontSize;
            FontName = settings.FontName;
        }

        public void WriteTo(Properties.Settings settings, ConfigReader reader)
        {
            settings.DisplayName = RememberMe ? DisplayName : String.Empty;
            settings.DisplayMessage = RememberMe ? DisplayMessage : String.Empty;
            settings.DisplayImage = RememberMe ? DisplayImage : null;
            settings.GroupName = RememberMe ? GroupName : String.Empty;
            settings.EmailAddress = RememberMe ? EmailAddress : String.Empty;
            settings.Username = RememberMe ? Username : String.Empty;
            settings.Password = ProtectedData.Protect(RememberMe ? Password : String.Empty);
            settings.Domain = RememberMe ? Domain : String.Empty;

            reader.SetSetting(SettingKey.AutoSignIn, AutoSignMeIn);
            reader.SetSetting(SettingKey.IdleTimeout, IdleTimeout);
            
            settings.FontColor = FontColor;
            settings.FontStyle = FontStyle;
            settings.FontSize = FontSize;
            settings.FontName = FontName;
        }
    }
}
