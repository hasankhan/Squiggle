using System;
using System.Windows;
using System.Windows.Controls;
using Squiggle.UI.Settings;
using System.Linq;

namespace Squiggle.UI.Controls
{   
    /// <summary>
    /// Interaction logic for SignInControl.xaml
    /// </summary>
    public partial class SignInControl : UserControl
    {
        public event EventHandler<LogInEventArgs> LoginInitiated = delegate { };

        public string GroupName
        {
            get { return txtGroupName.Text.Trim(); }
            set { txtGroupName.SelectedValue = value.Trim();  }
        }

        public string DisplayName
        {
            get { return txtdisplayName.Text.Trim(); }
            set { txtdisplayName.Text = value.Trim(); }
        }

        public SignInControl()
        {
            InitializeComponent();

            SettingsProvider.Current.SettingsUpdated += new EventHandler(Current_SettingsUpdated);
        }

        void Current_SettingsUpdated(object sender, EventArgs e)
        {
            LoadSettings(SettingsProvider.Current.Settings);
        }

        public void SetDisplayName(string name)
        {
            DisplayName = name;
            txtdisplayName.SelectAll();
        }

        public void SetGroupName(string name)
        {
            GroupName = name;
        }

        public void LoadGroups(ContactGroups groups)
        {
            txtGroupName.ItemsSource = groups;
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtdisplayName.Text.Trim()))
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
            SetDisplayName(settings.PersonalSettings.DisplayName);
            SetGroupName(settings.PersonalSettings.GroupName);
            LoadGroups(settings.ContactSettings.ContactGroups);
        }
    }

    public class LogInEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string GroupName { get; set; }
    }
}
