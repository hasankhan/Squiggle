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
    /// Interaction logic for DisplayMessageBlock.xaml
    /// </summary>
    public partial class DisplayMessageBlock : UserControl
    {
        Buddy self;

        public Buddy SelfUser 
        {
            get { return self; }
            set { self = value; ShowDsiplayMessage(); }
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

            Properties.Settings.Default.DisplayMessage = txtDisplayMessage.Text;
            Properties.Settings.Default.Save();

            if (txtDisplayMessage.Text.Trim() == String.Empty)
                emptyMessageView.Visibility = Visibility.Visible;
            else
                readOnlyMessageView.Visibility = Visibility.Visible;

            writableMessageView.Visibility = Visibility.Hidden;
        }

        private void ShowDsiplayMessage()
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.DisplayMessage))
            {
                SelfUser.DisplayMessage = Properties.Settings.Default.DisplayMessage;
                emptyMessageView.Visibility = Visibility.Hidden;
                readOnlyMessageView.Visibility = Visibility.Visible;
            }
        }
    }
}
