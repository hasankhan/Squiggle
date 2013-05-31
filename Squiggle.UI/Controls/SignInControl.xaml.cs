using System;
using System.Windows;
using System.Windows.Controls;
using Squiggle.UI.Settings;
using Squiggle.UI.ViewModel;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for SignInControl.xaml
    /// </summary>
    public partial class SignInControl : UserControl
    {
        public event EventHandler<LogInEventArgs> LoginInitiated = delegate { };
        SignInViewModel viewModel;

        public string GroupName
        {
            get { return viewModel.GroupName; }
            set { viewModel.GroupName = value; }
        }

        public string DisplayName
        {
            get { return viewModel.DisplayName; }
            set
            {
                viewModel.DisplayName = value;
                txtDisplayName.SelectAll();
            }
        }

        public bool RememberMe
        {
            get { return viewModel.RememberMe; }
            set { viewModel.RememberMe = value; }
        }

        public SignInControl()
        {
            InitializeComponent();
            DataContext = viewModel = new SignInViewModel();
        }

        public void LoadGroups(ContactGroups groups)
        {
            txtGroupName.ItemsSource = groups;
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            if (viewModel.AskDisplayName && String.IsNullOrEmpty(DisplayName))
                return;
            if (viewModel.AskDomain && String.IsNullOrEmpty(viewModel.Domain))
                return;
            if (viewModel.AskUsername && String.IsNullOrEmpty(viewModel.Username))
                return;

            var settings = SettingsProvider.Current.Settings.PersonalSettings;

            if (!viewModel.RememberMe ||
                (viewModel.AskDisplayName && settings.DisplayName != DisplayName) ||
                (viewModel.AskUsername && settings.Username != viewModel.Username))
                ResetPersonalSettings(settings);

            if (viewModel.RememberMe)
            {
                settings.DisplayName = DisplayName;
                settings.GroupName = GroupName;
                settings.Username = viewModel.Username;
                settings.Password = txtPassword.Password;
                settings.Domain = viewModel.Domain;
            }

            settings.RememberMe = viewModel.RememberMe;
            settings.AutoSignMeIn = viewModel.AutoSignIn;
            if (!String.IsNullOrEmpty(GroupName))
                SettingsProvider.Current.Settings.ContactSettings.ContactGroups.Add(GroupName);
            txtGroupName.SelectedValue = GroupName;

            SettingsProvider.Current.Save();

            LoginInitiated(this, new LogInEventArgs()
            {
                DisplayName = DisplayName,
                GroupName = GroupName,
                Domain = viewModel.Domain,
                Username = viewModel.Username,
                Password = txtPassword.Password
            });
        }

        static void ResetPersonalSettings(PersonalSettings settings)
        {
            settings.DisplayName = String.Empty;
            settings.DisplayMessage = String.Empty;
            settings.GroupName = String.Empty;
            settings.EmailAddress = String.Empty;
            settings.DisplayImage = null;
            settings.Username = String.Empty;
            settings.Password = String.Empty;
            settings.Domain = String.Empty;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtDisplayName.Focus();
        }

        private void txtGroupName_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete &&
                txtGroupName.SelectedItem != null) // if it exists in combo list items
            {
                SettingsProvider.Current.Settings.ContactSettings.ContactGroups.Remove((ContactGroup)txtGroupName.SelectedItem);
                SettingsProvider.Current.Save();
            }
        }

        internal void LoadSettings(SquiggleSettings settings)
        {
            viewModel.AutoSignIn = settings.PersonalSettings.AutoSignMeIn;
            viewModel.RememberMe = settings.PersonalSettings.RememberMe;

            if (viewModel.RememberMe)
            {
                viewModel.Username = settings.PersonalSettings.Username;
                txtPassword.Password = settings.PersonalSettings.Password;
                viewModel.Domain = settings.PersonalSettings.Domain;
                DisplayName = settings.PersonalSettings.DisplayName;
                GroupName = settings.PersonalSettings.GroupName;
            }
            LoadGroups(settings.ContactSettings.ContactGroups);
        }

        internal void Configure(Plugins.Authentication.IAuthenticationProvider authProvider)
        {
            viewModel.AskDisplayName = !authProvider.ReturnsDisplayName;
            viewModel.AskGroupName = !authProvider.ReturnsDisplayName;

            viewModel.AskDomain = authProvider.RequiresDomain;
            viewModel.AskPassword = authProvider.RequiresPassword;
            viewModel.AskUsername = authProvider.RequiresUsername;
        }
    }

    public class LogInEventArgs : EventArgs
    {
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignInViewModel : ViewModelBase
    {
        bool _rememberMe;
        public bool RememberMe
        {
            get { return _rememberMe; }
            set { Set(() => RememberMe, ref _rememberMe, value); }
        }

        bool _autoSignIn;
        public bool AutoSignIn
        {
            get { return _autoSignIn; }
            set { Set(() => AutoSignIn, ref _autoSignIn, value); }
        }

        bool _askUsername;
        public bool AskUsername
        {
            get { return _askUsername; }
            set
            {
                Set(() => AskUsername, ref _askUsername, value);
                OnPropertyChanged(()=>SingleSignOn);
            }
        }

        string _username;
        public string Username
        {
            get { return _username; }
            set { Set(() => Username, ref _username, Trim(value)); }
        }

        bool _askPassword;
        public bool AskPassword
        {
            get { return _askPassword; }
            set
            {
                Set(() => AskPassword, ref _askPassword, value);
                OnPropertyChanged(()=>SingleSignOn);
            }
        }

        bool _askDisplayName;
        public bool AskDisplayName
        {
            get { return _askDisplayName; }
            set
            {
                Set(() => AskDisplayName, ref _askDisplayName, value);
                OnPropertyChanged(()=>SingleSignOn);
            }
        }

        string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { Set(() => DisplayName, ref _displayName, Trim(value)); }
        }

        bool _askDomain;
        public bool AskDomain
        {
            get { return _askDomain; }
            set
            {
                Set(() => AskDomain, ref _askDomain, value);
                OnPropertyChanged(()=>SingleSignOn);
            }
        }

        string _domain;
        public string Domain
        {
            get { return _domain; }
            set { Set(() => Domain, ref _domain, Trim(value)); }
        }

        bool _askGroupName;
        public bool AskGroupName
        {
            get { return _askGroupName; }
            set
            {
                Set(() => AskGroupName, ref _askGroupName, value);
                OnPropertyChanged(()=>SingleSignOn);
            }
        }

        string _groupName;
        public string GroupName
        {
            get { return _groupName ?? String.Empty; }
            set
            {
                Set(() => GroupName, ref _groupName, Trim(value));
                OnPropertyChanged(()=>SingleSignOn);
            }
        }

        public bool SingleSignOn
        {
            get
            {
                return
                       !AskDisplayName
                    && !AskUsername
                    && !AskDomain
                    && !AskGroupName
                    && !AskPassword;
            }
            set
            {
                AskDisplayName = false;
                AskUsername = false;
                AskDomain = false;
                AskGroupName = false;
                AskPassword = false;

                OnPropertyChanged(()=>SingleSignOn);
            }
        }
        string Trim(string value)
        {
            return (value ?? String.Empty).Trim();
        }
    }
}
