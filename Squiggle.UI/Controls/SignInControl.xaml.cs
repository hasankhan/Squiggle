using System;
using System.Linq;
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
            get { return viewModel.GroupName.Trim(); }
            set { viewModel.GroupName = value.Trim();  }
        }

        public string DisplayName
        {
            get { return viewModel.DisplayName.Trim(); }
            set 
            { 
                viewModel.DisplayName = value.Trim();
                txtdisplayName.SelectAll();
            }
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
            if (String.IsNullOrEmpty(DisplayName))
                return;

            var settings = SettingsProvider.Current.Settings;

            if (chkRememberName.IsChecked.GetValueOrDefault())
            {
                // reset the display message if display name changes for saved user
                if (settings.PersonalSettings.RememberMe && settings.PersonalSettings.DisplayName != DisplayName)
                    ResetPersonalSettings(settings);

                settings.PersonalSettings.DisplayName = DisplayName;
                settings.PersonalSettings.GroupName = GroupName;
            }
            else
                ResetPersonalSettings(settings);

            settings.PersonalSettings.RememberMe = chkRememberName.IsChecked.GetValueOrDefault();
            settings.PersonalSettings.AutoSignMeIn = chkAutoSignIn.IsChecked.GetValueOrDefault();
            if (!String.IsNullOrEmpty(GroupName))
                settings.ContactSettings.ContactGroups.Add(GroupName);
            txtGroupName.SelectedValue = GroupName;

            SettingsProvider.Current.Save();

            LoginInitiated(this, new LogInEventArgs()
            {
                UserName = DisplayName,
                GroupName = GroupName
            });
        }

        static void ResetPersonalSettings(SquiggleSettings settings)
        {
            settings.PersonalSettings.DisplayName = String.Empty;
            settings.PersonalSettings.DisplayMessage = String.Empty;
            settings.PersonalSettings.GroupName = String.Empty;
            settings.PersonalSettings.EmailAddress = String.Empty;
            settings.PersonalSettings.DisplayImage = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtdisplayName.Focus();
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
            chkAutoSignIn.IsChecked = settings.PersonalSettings.AutoSignMeIn;
            chkRememberName.IsChecked = settings.PersonalSettings.RememberMe;
            DisplayName = settings.PersonalSettings.DisplayName;
            GroupName = settings.PersonalSettings.GroupName;
            LoadGroups(settings.ContactSettings.ContactGroups);
        }

        internal void Configure(Plugins.Authentication.IAuthenticationProvider authProvider)
        {
            viewModel.AskDisplayName = !authProvider.ReturnsDisplayName;
            viewModel.AskGroupName = !authProvider.ReturnsDisplayName;

            viewModel.AskDomain = authProvider.ReturnsDisplayName;
            viewModel.AskPassword = authProvider.RequiresPassword;
            viewModel.AskUsername = authProvider.RequiresUsername;
        }
    }

    public class LogInEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string GroupName { get; set; }
    }

    public class SignInViewModel : ViewModelBase
    {
        bool _askUsername;
        public bool AskUsername
        {
            get { return _askUsername; }
            set { Set(()=>AskUsername, ref _askUsername, value); }
        }

        string _Username;
        public string Username
        {
            get { return _Username; }
            set { Set(()=>Username, ref _Username, value); }
        }

        bool _askPassword;
        public bool AskPassword
        {
            get { return _askPassword; }
            set { Set(()=>AskPassword, ref _askPassword, value); }
        }

        string _Password;
        public string Password
        {
            get { return _Password; }
            set { Set(()=>Password, ref _Password, value); }
        }

        bool _askDisplayName;
        public bool AskDisplayName
        {
            get { return _askDisplayName; }
            set { Set(()=>AskDisplayName, ref _askDisplayName, value); }
        }
        
        string _DisplayName;
        public string DisplayName
        {
            get { return _DisplayName; }
            set { Set(()=>DisplayName, ref _DisplayName, value); }
        }

        bool _askDomain;
        public bool AskDomain
        {
            get { return _askDomain; }
            set { Set(()=>AskDomain, ref _askDomain, value); }
        }

        string _Domain;
        public string Domain
        {
            get { return _Domain; }
            set { Set(()=>Domain, ref _Domain, value); }
        }

        bool _askGroupName;
        public bool AskGroupName
        {
            get { return _askGroupName; }
            set { Set(()=>AskGroupName, ref _askGroupName, value); }
        }

        string _GroupName;
        public string GroupName
        {
            get { return _GroupName; }
            set { Set(()=>GroupName, ref _GroupName, value); }
        }
    }
}
