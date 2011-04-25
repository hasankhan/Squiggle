
namespace Squiggle.UI.Settings
{
    class SquiggleSettings
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
    }    
}
