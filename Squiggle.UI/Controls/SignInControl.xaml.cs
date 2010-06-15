using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Squiggle.UI.Settings;

namespace Squiggle.UI.Controls
{   
    /// <summary>
    /// Interaction logic for SignInControl.xaml
    /// </summary>
    public partial class SignInControl : UserControl
    {
        public event EventHandler<LogInEventArgs> CredentialsVerfied = delegate { };

        public SignInControl()
        {
            InitializeComponent();
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtdisplayName.Text.Trim()))
                return;

            var settings = SettingsProvider.Current.Settings;

            if (chkRememberName.IsChecked.GetValueOrDefault())
                settings.PersonalSettings.DisplayName = txtdisplayName.Text;
            else
                settings.PersonalSettings.DisplayName = String.Empty;

            settings.PersonalSettings.RememberMe = chkRememberName.IsChecked.GetValueOrDefault();
            settings.PersonalSettings.AutoSignMeIn = chkAutoSignIn.IsChecked.GetValueOrDefault();
            SettingsProvider.Current.Save();

            CredentialsVerfied(this, new LogInEventArgs() { UserName = txtdisplayName.Text.Trim() });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtdisplayName.Focus();
        }
    }

    public class LogInEventArgs : EventArgs
    {
        public string UserName { get; set; }
    }
}
