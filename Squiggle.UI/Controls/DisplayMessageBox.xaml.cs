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
using Squiggle.Chat;

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
                ShowAppropriateControl();
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
            ShowAppropriateControl();
        }

        private void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowAppropriateControl();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ShowAppropriateControl();
                e.Handled = true;
            }
        }

        private void ForceUpdate()
        {
            BindingExpression expression = txtMessage.GetBindingExpression(TextBox.TextProperty);
            if (expression != null)
                expression.UpdateSource();
        }

        private void ShowAppropriateControl()
        {
            if (String.IsNullOrEmpty(SelfUser.DisplayMessage))
                ShowDefaultMessage();
            else
                ShowReadOnlyMessage();
        }

        private void ShowReadOnlyMessage()
        {
            txbMessage.Visibility = Visibility.Visible;
            txtMessage.Visibility = Visibility.Hidden;
        }

        private void ShowDefaultMessage()
        {
            txbMessage.Visibility = Visibility.Hidden;
            txtMessage.Visibility = Visibility.Hidden;
        }
    }
}
