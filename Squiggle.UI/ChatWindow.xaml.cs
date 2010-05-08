using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var title = new Bold(new Run("Someone Says: "));
            var text = new Run(txtMessage.Text);
            sentMessages.Inlines.Add(title);
            sentMessages.Inlines.Add(text);
            sentMessages.Inlines.Add(new Run("\r\n"));
            scrollViewer.ScrollToBottom();
            txtMessage.Text = String.Empty;
            txtMessage.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
