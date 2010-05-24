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
using Squiggle.UI.Settings;

namespace Squiggle.UI.Controls
{
    
    /// <summary>
    /// Interaction logic for DisplayMessageBlock.xaml
    /// </summary>
    public partial class DisplayMessageBlock : UserControl
    {
        Buddy self;

        public Buddy SelfUser 
        {
            get { return self; }
            set { self = value; ShowDisplayMessage(); }
        }
       
        public DisplayMessageBlock()
        {
            InitializeComponent();
        }

        void PropmtDisplayMessage(object sender, RoutedEventArgs e)
        {
            readOnlyMessageView.Visibility = Visibility.Hidden;
            emptyMessageView.Visibility = Visibility.Hidden;
            writableMessageView.Visibility = Visibility.Visible;

            txtDisplayMessage.Text = SelfUser.DisplayMessage;
            txtDisplayMessage.Focus();

        }

        void UpdateDisplayMessage(object sender, RoutedEventArgs e)
        {
            SelfUser.DisplayMessage = txtDisplayMessage.Text;

            SettingsProvider.Current.Update(s => s.PersonalSettings.DisplayName = txtDisplayMessage.Text);

            if (txtDisplayMessage.Text.Trim() == String.Empty)
                emptyMessageView.Visibility = Visibility.Visible;
            else
                readOnlyMessageView.Visibility = Visibility.Visible;

            writableMessageView.Visibility = Visibility.Hidden;
        }

        private void ShowDisplayMessage()
        {
            string message = SettingsProvider.Current.Settings.PersonalSettings.DisplayMessage;
            if (!String.IsNullOrEmpty(message))
            {
                SelfUser.DisplayMessage = message;
                emptyMessageView.Visibility = Visibility.Hidden;
                readOnlyMessageView.Visibility = Visibility.Visible;
            }
        }
    }
}
