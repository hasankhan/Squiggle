using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Squiggle.Chat;
using Squiggle.UI.Settings;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for DisplayMessageBox.xaml
    /// </summary>
    public partial class DisplayMessageBox : UserControl
    {
        public static DependencyProperty SelfUserProperty = DependencyProperty.Register("SelfUser", typeof(Buddy), typeof(DisplayMessageBox), new PropertyMetadata(null));
        public Buddy SelfUser
        {
            get { return GetValue(SelfUserProperty) as Buddy; }
            set
            {
                SetValue(SelfUserProperty, value);
                ShowReadOnlyMessage();
            }
        } 

        public DisplayMessageBox()
        {
            InitializeComponent();
        }

        private void txbMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowEditableMessage();

            txtMessage.Focus();
        }

        private void ShowEditableMessage()
        {
            txbMessage.Visibility = Visibility.Hidden;
            txtMessage.Visibility = Visibility.Visible;
        }

        private void txtMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            ShowReadOnlyMessage();
        }

        private void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ForceUpdate();
                ShowReadOnlyMessage();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ShowReadOnlyMessage();
                e.Handled = true;
            }
        }

        private void ForceUpdate()
        {
            BindingExpression expression = txtMessage.GetBindingExpression(TextBox.TextProperty);
            if (expression != null)
                expression.UpdateSource();

            if (SettingsProvider.Current.Settings.PersonalSettings.RememberMe)
            {
                SettingsProvider.Current.Settings.PersonalSettings.DisplayMessage = txtMessage.Text;
                SettingsProvider.Current.Save();
            }
        }

        private void ShowReadOnlyMessage()
        {
            txbMessage.Visibility = Visibility.Visible;
            txtMessage.Visibility = Visibility.Hidden;
        }
    }
}
