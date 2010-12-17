using System;
using System.Windows;
using System.Windows.Controls;
using Squiggle.UI.Settings;

namespace Squiggle.UI.Controls
{   
    /// <summary>
    /// Interaction logic for SignInControl.xaml
    /// </summary>
    public partial class SignInControl : UserControl
    {
        public event EventHandler<LogInEventArgs> LoginInitiated = delegate { };

        public SignInControl()
        {
            InitializeComponent();
        }

        public void SetDisplayName(string name)
        {
            txtdisplayName.Text = name;
            txtdisplayName.SelectAll();
        }

        public void SetGroupName(string name)
        {
            txtGroupName.Text = name;
        }

        public void LoadGroups(System.Collections.Generic.IEnumerable<string> groupNames)
        {
            txtGroupName.ItemsSource = groupNames;
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtdisplayName.Text.Trim()))
                return;

            var settings = SettingsProvider.Current.Settings;
            
            string displayName = txtdisplayName.Text.Trim();
            string groupName = txtGroupName.Text.Trim();

            if (chkRememberName.IsChecked.GetValueOrDefault())
            {
                // reset the display message if display name changes for saved user
                if (settings.PersonalSettings.RememberMe &&
                    settings.PersonalSettings.DisplayName != displayName)
                    settings.PersonalSettings.DisplayMessage = String.Empty;

                settings.PersonalSettings.DisplayName = displayName;
                settings.PersonalSettings.GroupName = groupName;
            }
            else
            {
                settings.PersonalSettings.DisplayName = settings.PersonalSettings.DisplayMessage = String.Empty;
                settings.PersonalSettings.GroupName = settings.PersonalSettings.GroupName = String.Empty;
            }

            settings.PersonalSettings.RememberMe = chkRememberName.IsChecked.GetValueOrDefault();
            settings.PersonalSettings.AutoSignMeIn = chkAutoSignIn.IsChecked.GetValueOrDefault();
            if (!String.IsNullOrEmpty(groupName))
                settings.GeneralSettings.Groups.Add(groupName);

            SettingsProvider.Current.Save();

            LoginInitiated(this, new LogInEventArgs()
            {
                UserName = displayName,
                GroupName = groupName
            });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtdisplayName.Focus();
        }        
    }

    public class LogInEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string GroupName { get; set; }
    }
}
