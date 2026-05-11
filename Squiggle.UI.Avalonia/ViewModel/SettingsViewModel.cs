namespace Squiggle.UI.Avalonia.ViewModel;

public class SettingsViewModel : ViewModelBase
{
    public GeneralSettingsViewModel GeneralSettings { get; set; } = new();
    public ConnectionSettingsViewModel ConnectionSettings { get; set; } = new();
    public PersonalSettingsViewModel PersonalSettings { get; set; } = new();
    public ChatSettingsViewModel ChatSettings { get; set; } = new();
    public ContactSettingsViewModel ContactSettings { get; set; } = new();
}

public class GeneralSettingsViewModel : ViewModelBase
{
    private bool _hideToSystemTray;
    public bool HideToSystemTray { get => _hideToSystemTray; set => Set(ref _hideToSystemTray, value); }

    private bool _showPopups = true;
    public bool ShowPopups { get => _showPopups; set => Set(ref _showPopups, value); }

    private bool _audioAlerts = true;
    public bool AudioAlerts { get => _audioAlerts; set => Set(ref _audioAlerts, value); }

    private string _downloadsFolder = "";
    public string DownloadsFolder { get => _downloadsFolder; set => Set(ref _downloadsFolder, value); }

    private bool _enableStatusLogging;
    public bool EnableStatusLogging { get => _enableStatusLogging; set => Set(ref _enableStatusLogging, value); }

    private bool _checkForUpdates = true;
    public bool CheckForUpdates { get => _checkForUpdates; set => Set(ref _checkForUpdates, value); }
}

public class ConnectionSettingsViewModel : ViewModelBase
{
    private string _presenceAddress = "224.1.1.1";
    public string PresenceAddress { get => _presenceAddress; set => Set(ref _presenceAddress, value); }

    private int _presencePort = 9999;
    public int PresencePort { get => _presencePort; set => Set(ref _presencePort, value); }

    private int _chatPort = 9998;
    public int ChatPort { get => _chatPort; set => Set(ref _chatPort, value); }

    private string _bindToIP = "";
    public string BindToIP { get => _bindToIP; set => Set(ref _bindToIP, value); }

    private int _keepAliveTime = 5000;
    public int KeepAliveTime { get => _keepAliveTime; set => Set(ref _keepAliveTime, value); }
}

public class PersonalSettingsViewModel : ViewModelBase
{
    private string _displayName = "";
    public string DisplayName { get => _displayName; set => Set(ref _displayName, value); }

    private string _displayMessage = "";
    public string DisplayMessage { get => _displayMessage; set => Set(ref _displayMessage, value); }

    private string _groupName = "";
    public string GroupName { get => _groupName; set => Set(ref _groupName, value); }

    private string _emailAddress = "";
    public string EmailAddress { get => _emailAddress; set => Set(ref _emailAddress, value); }

    private string? _displayImage;
    public string? DisplayImage { get => _displayImage; set => Set(ref _displayImage, value); }

    private bool _rememberMe;
    public bool RememberMe { get => _rememberMe; set => Set(ref _rememberMe, value); }

    private bool _autoSignMeIn;
    public bool AutoSignMeIn { get => _autoSignMeIn; set => Set(ref _autoSignMeIn, value); }

    private int _idleTimeout = 5;
    public int IdleTimeout { get => _idleTimeout; set => Set(ref _idleTimeout, value); }
}

public class ChatSettingsViewModel : ViewModelBase
{
    private bool _showEmoticons = true;
    public bool ShowEmoticons { get => _showEmoticons; set => Set(ref _showEmoticons, value); }

    private bool _spellCheck;
    public bool SpellCheck { get => _spellCheck; set => Set(ref _spellCheck, value); }

    private bool _stealFocusOnNewMessage;
    public bool StealFocusOnNewMessage { get => _stealFocusOnNewMessage; set => Set(ref _stealFocusOnNewMessage, value); }

    private bool _clearChatOnWindowClose;
    public bool ClearChatOnWindowClose { get => _clearChatOnWindowClose; set => Set(ref _clearChatOnWindowClose, value); }

    private bool _enableLogging = true;
    public bool EnableLogging { get => _enableLogging; set => Set(ref _enableLogging, value); }
}

public class ContactSettingsViewModel : ViewModelBase
{
    private string _contactListSortField = "Status";
    public string ContactListSortField { get => _contactListSortField; set => Set(ref _contactListSortField, value); }

    private bool _groupContacts = true;
    public bool GroupContacts { get => _groupContacts; set => Set(ref _groupContacts, value); }

    private bool _showOfflineContacts = true;
    public bool ShowOfflineContacts { get => _showOfflineContacts; set => Set(ref _showOfflineContacts, value); }

    private string _contactListView = "Normal";
    public string ContactListView { get => _contactListView; set => Set(ref _contactListView, value); }
}
