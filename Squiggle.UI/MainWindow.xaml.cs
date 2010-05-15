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

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        short chatPort = 7778;
        short presencePort = 9999;
        const int keepAliveTimeout = 20000;
        ChatClient chatClient;
        ChatViewModel chatVM;
        
        public MainWindow()
        {
            InitializeComponent();
            chatPort = (short)Utility.GetFreePort();
            var ipAddress = Utility.GetLocalIPAddress();
            chatClient = new ChatClient(new IPEndPoint(ipAddress, chatPort), presencePort, keepAliveTimeout);
            chatClient.Login("Ali");
            chatClient.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            chatVM = new ChatViewModel(chatClient);
            this.DataContext = chatVM;
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            ChatWindow window = new ChatWindow();
            window.DataContext = e.Session;
            window.Show();
        }       

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Buddy buddy= ((TextBlock)sender).Tag as Buddy;
            ChatWindow window = new ChatWindow();
            window.Title = buddy.DisplayName;
            window.DataContext = buddy.StartChat();
            window.Show();
        }
    }
}
