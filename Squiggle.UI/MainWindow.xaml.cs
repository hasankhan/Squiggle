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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Squiggle.Chat;
using System.Net;
using System.ComponentModel;
using System.Threading;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const short chatPort = 7777;
        const short presencePort = 9999;
        const int keepAliveTimeout = 20000;
        ChatClient chatClient;
        
        public MainWindow()
        {
            InitializeComponent();
            var ipAddress = Utility.GetLocalIPAddress();
            chatClient = new ChatClient(new IPEndPoint(ipAddress, chatPort), presencePort, keepAliveTimeout);
            chatClient.Login(Dns.GetHostName());

            this.DataContext = chatClient;
            signedInView.Visibility = Visibility.Visible;
            signedOffView.Visibility = Visibility.Hidden;
        }       

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Buddy buddy= ((TextBlock)sender).Tag as Buddy;
            ChatWindow window = new ChatWindow();
            window.Title = buddy.DisplayName;
            window.DataContext = chatClient.StartChat(buddy.Address);
            window.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
