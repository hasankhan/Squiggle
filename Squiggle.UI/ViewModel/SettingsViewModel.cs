using System.Collections.Generic;
using System.Net;
using Squiggle.UI.Settings;

namespace Squiggle.UI.ViewModel
{
    class SettingsViewModel
    {
        SquiggleSettings settings;

        public GeneralSettingsViewModel GeneralSettings { get; set; }
        public ConnectionSettingsViewModel ConnectionSettings { get; set; }
        public PersonalSettingsViewModel PersonalSettings { get; set; }
        public ChatSettingsViewModel ChatSettings { get; set; }
        public ContactSettingsViewModel ContactSettings { get; set; }

        public SettingsViewModel()
        {
            GeneralSettings = new GeneralSettingsViewModel();
            ConnectionSettings = new ConnectionSettingsViewModel();
            PersonalSettings = new PersonalSettingsViewModel();
            ChatSettings = new ChatSettingsViewModel();
            ContactSettings = new ContactSettingsViewModel();
        }

        public SettingsViewModel(SquiggleSettings settings): this()
        {
            this.settings = settings;

            ChatSettings.ShowEmoticons = settings.ChatSettings.ShowEmoticons;
            GeneralSettings.HideToSystemTray = settings.GeneralSettings.HideToSystemTray;
            GeneralSettings.ShowPopups = settings.GeneralSettings.ShowPopups;
            GeneralSettings.AudioAlerts = settings.GeneralSettings.AudioAlerts;
            GeneralSettings.DownloadsFolder = settings.GeneralSettings.DownloadsFolder;
            GeneralSettings.EnableStatusLogging = settings.GeneralSettings.EnableStatusLogging;
            GeneralSettings.CheckForUpdates = settings.GeneralSettings.CheckForUpdates;

            ChatSettings.SpellCheck = settings.ChatSettings.SpellCheck;
            ChatSettings.StealFocusOnNewMessage = settings.ChatSettings.StealFocusOnNewMessage;
            ChatSettings.ClearChatOnWindowClose = settings.ChatSettings.ClearChatOnWindowClose;
            ChatSettings.EnableLogging = settings.ChatSettings.EnableLogging;
            
            ContactSettings.ContactListSortField = settings.ContactSettings.ContactListSortField;
            ContactSettings.GroupContacts = settings.ContactSettings.GroupContacts;
            ContactSettings.ShowOfflineContacts = settings.ContactSettings.ShowOfflineContacts;
            ContactSettings.ContactListView = settings.ContactSettings.ContactListView;

            ConnectionSettings.BindToIP = settings.ConnectionSettings.BindToIP;
            ConnectionSettings.ChatPort = settings.ConnectionSettings.ChatPort;
            ConnectionSettings.KeepAliveTime = settings.ConnectionSettings.KeepAliveTime;
            ConnectionSettings.PresenceAddress = settings.ConnectionSettings.PresenceAddress;
            ConnectionSettings.PresencePort = settings.ConnectionSettings.PresencePort;

            PersonalSettings.DisplayMessage = settings.PersonalSettings.DisplayMessage;
            PersonalSettings.DisplayImage = settings.PersonalSettings.DisplayImage;
            PersonalSettings.DisplayName = settings.PersonalSettings.DisplayName;
            PersonalSettings.EmailAddress = settings.PersonalSettings.EmailAddress;
            PersonalSettings.GroupName = settings.PersonalSettings.GroupName;
            PersonalSettings.RememberMe = settings.PersonalSettings.RememberMe;
            PersonalSettings.AutoSignMeIn = settings.PersonalSettings.AutoSignMeIn;
            PersonalSettings.IdleTimeout = settings.PersonalSettings.IdleTimeout;
        }

        public void Update()
        {
            if (settings == null)
                return;

            settings.ChatSettings.ShowEmoticons = ChatSettings.ShowEmoticons;
            settings.GeneralSettings.EnableStatusLogging = GeneralSettings.EnableStatusLogging;
            settings.GeneralSettings.CheckForUpdates = GeneralSettings.CheckForUpdates;
            settings.GeneralSettings.HideToSystemTray = GeneralSettings.HideToSystemTray;
            settings.GeneralSettings.ShowPopups = GeneralSettings.ShowPopups;
            settings.GeneralSettings.AudioAlerts = GeneralSettings.AudioAlerts;
            settings.ChatSettings.StealFocusOnNewMessage = ChatSettings.StealFocusOnNewMessage;
            settings.ChatSettings.ClearChatOnWindowClose = ChatSettings.ClearChatOnWindowClose;
            settings.ChatSettings.SpellCheck = ChatSettings.SpellCheck;
            settings.ChatSettings.EnableLogging = ChatSettings.EnableLogging;
            settings.ContactSettings.GroupContacts = ContactSettings.GroupContacts;
            settings.ContactSettings.ContactListSortField = ContactSettings.ContactListSortField;
            settings.ContactSettings.ShowOfflineContacts = ContactSettings.ShowOfflineContacts;
            settings.ContactSettings.ContactListView = ContactSettings.ContactListView;
            settings.GeneralSettings.DownloadsFolder = GeneralSettings.DownloadsFolder;

            settings.ConnectionSettings.BindToIP = ConnectionSettings.BindToIP;
            settings.ConnectionSettings.ChatPort = ConnectionSettings.ChatPort;
            settings.ConnectionSettings.KeepAliveTime = ConnectionSettings.KeepAliveTime;
            settings.ConnectionSettings.PresenceAddress = ConnectionSettings.PresenceAddress;
            settings.ConnectionSettings.PresencePort = ConnectionSettings.PresencePort;

            settings.PersonalSettings.DisplayMessage = PersonalSettings.DisplayMessage;
            settings.PersonalSettings.DisplayImage = PersonalSettings.DisplayImage;
            settings.PersonalSettings.GroupName = PersonalSettings.GroupName;
            settings.PersonalSettings.DisplayName = PersonalSettings.DisplayName;
            settings.PersonalSettings.IdleTimeout = PersonalSettings.IdleTimeout;
            settings.PersonalSettings.RememberMe = PersonalSettings.RememberMe;
            settings.PersonalSettings.AutoSignMeIn = PersonalSettings.AutoSignMeIn;
            settings.PersonalSettings.EmailAddress = PersonalSettings.EmailAddress;
        }
    }

    class PersonalSettingsViewModel: ViewModelBase
    {
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public string DisplayMessage { get; set; }
        public string EmailAddress { get; set; }
        public int IdleTimeout { get; set; }
        public bool RememberMe { get; set; }
        public bool AutoSignMeIn { get; set; }

        byte[] displayImage;
        public byte[] DisplayImage 
        {
            get { return displayImage; }
            set { Set(() => DisplayImage, ref displayImage, value); }
        }
    }

    class GeneralSettingsViewModel
    {
        public bool RunAtStartup { get; set; }
        public bool HideToSystemTray { get; set; }
        public bool ShowPopups { get; set; }
        public string DownloadsFolder { get; set; }
        public bool AudioAlerts { get; set; }
        public bool EnableStatusLogging { get; set; }
        public bool CheckForUpdates { get; set; }
    }

    class ChatSettingsViewModel
    {
        public bool EnableLogging { get; set; }
        public bool SpellCheck { get; set; }
        public bool ShowEmoticons { get; set; }
        public bool StealFocusOnNewMessage { get; set; }
        public bool ClearChatOnWindowClose { get; set; }
    }

    class ContactSettingsViewModel
    {
        public ContactListSortField ContactListSortField { get; set; }
        public bool GroupContacts { get; set; }
        public bool ShowOfflineContacts { get; set; }
        public ContactListView ContactListView { get; set; }
    }

    class ConnectionSettingsViewModel: ViewModelBase
    {
        string presenceAddress;
        public string PresenceAddress
        {
            get { return presenceAddress; }
            set { Set(()=>PresenceAddress, ref presenceAddress, value); }
        }
        public int PresencePort { get; set; }
        public int ChatPort { get; set; }
        public int KeepAliveTime { get; set; }
        public List<string> AllIPs { get; private set; }
        public string BindToIP { get; set; }

        public ConnectionSettingsViewModel()
        {
            AllIPs = new List<string>();
        }
    }
}
