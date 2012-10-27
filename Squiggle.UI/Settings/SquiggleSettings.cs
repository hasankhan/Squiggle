using Squiggle.Utilities.Application;

namespace Squiggle.UI.Settings
{
    class SquiggleSettings : ISettingsGroup
    {
        public GeneralSettings GeneralSettings { get; set; }
        public ConnectionSettings ConnectionSettings { get; set; }
        public PersonalSettings PersonalSettings { get; set; }
        public ChatSettings ChatSettings { get; set; }
        public ContactSettings ContactSettings { get; set; }

        public SquiggleSettings()
        {
            GeneralSettings = new GeneralSettings();
            ConnectionSettings = new ConnectionSettings();
            PersonalSettings = new PersonalSettings();
            ChatSettings = new ChatSettings();
            ContactSettings = new ContactSettings();
        }

        public void ReadFrom(Properties.Settings settings, ConfigReader reader)
        {
            GeneralSettings.ReadFrom(settings, reader);
            ConnectionSettings.ReadFrom(settings, reader);
            PersonalSettings.ReadFrom(settings, reader);
            ChatSettings.ReadFrom(settings, reader);
            ContactSettings.ReadFrom(settings, reader);
        }

        public void WriteTo(Properties.Settings settings, ConfigReader reader)
        {
            GeneralSettings.WriteTo(settings, reader);
            ConnectionSettings.WriteTo(settings, reader);
            PersonalSettings.WriteTo(settings, reader);
            ChatSettings.WriteTo(settings, reader);
            ContactSettings.WriteTo(settings, reader);
        }
    }    
}
