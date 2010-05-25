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

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for DisplayMessageBox.xaml
    /// </summary>
    public partial class DisplayMessageBox : UserControl
    {
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DisplayMessageBox), new PropertyMetadata(null));
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static DependencyProperty DefaultMessageProperty = DependencyProperty.Register("DafaultMessage", typeof(string), typeof(DisplayMessageBox), new PropertyMetadata(null));
        public string DefaultMessage
        {
            get { return GetValue(DefaultMessageProperty) as string; }
            set
            {
                SetValue(DefaultMessageProperty, value);
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
            txbDefaultMessage.Visibility = Visibility.Hidden;
            txtMessage.Visibility = Visibility.Visible;
        }

        private void txtMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            ForceUpdate(); 
            ShowAppropriateControl();
        }

        private void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ForceUpdate();
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

        private void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {
            ShowAppropriateControl();
        }

        private void ShowAppropriateControl()
        {
            if (String.IsNullOrEmpty(Text))
                ShowDefaultMessage();
            else
                ShowReadOnlyMessage();
        }

        private void ShowReadOnlyMessage()
        {
            txbMessage.Visibility = Visibility.Visible;
            txbDefaultMessage.Visibility = Visibility.Hidden;
            txtMessage.Visibility = Visibility.Hidden;
        }

        private void ShowDefaultMessage()
        {
            txbDefaultMessage.Visibility = Visibility.Visible;
            txbMessage.Visibility = Visibility.Hidden;
            txtMessage.Visibility = Visibility.Hidden;
        }
    }
}
