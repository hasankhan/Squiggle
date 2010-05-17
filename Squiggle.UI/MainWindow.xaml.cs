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
using StackOverflowClient;
using System.Windows.Controls.Primitives;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        short presencePort = 9999;
        TimeSpan keepAliveTimeout = 2.Seconds();
        IChatClient chatClient;
        ChatViewModel chatVM;
        
        public MainWindow()
        {
            InitializeComponent();

            if (!String.IsNullOrEmpty(Properties.Settings.Default.DisplayName))
                SignIn(Properties.Settings.Default.DisplayName);
            else
                txtdisplayName.Focus();
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            CreateChatWindow(e.Buddy, e.Message, e.Chat);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Buddy buddy = ((TextBlock)sender).Tag as Buddy;
            CreateChatWindow(buddy, string.Empty, buddy.StartChat());
        }

        static void CreateChatWindow(Buddy buddy, string message, IChat session)
        {
            ChatWindow window = new ChatWindow(buddy, message);
            window.Title = buddy.DisplayName;
            window.DataContext = session;
            window.Topmost = true;
            window.Show();
        }   

        private void SignIn(object sender, RoutedEventArgs e)
        {
            SignIn(txtdisplayName.Text);

            if (chkRememberName.IsChecked.HasValue && chkRememberName.IsChecked.Value)
                Properties.Settings.Default.DisplayName = txtdisplayName.Text;
            else
                Properties.Settings.Default.DisplayName = String.Empty;

            Properties.Settings.Default.Save();
        }

        private void SignIn(string displayName)
        {
            int chatPort = NetworkUtility.GetFreePort();
            var ipAddress = NetworkUtility.GetLocalIPAddress();
            chatClient = new ChatClient(new IPEndPoint(ipAddress, chatPort), presencePort, keepAliveTimeout);
            chatClient.Login(displayName);
            chatClient.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            chatClient.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            chatVM = new ChatViewModel(chatClient);
            this.DataContext = chatVM;

            if (!String.IsNullOrEmpty(Properties.Settings.Default.DisplayMessage))
            {
                chatVM.LoggedInUser.DisplayMessage = Properties.Settings.Default.DisplayMessage;
                emptyMessageView.Visibility = Visibility.Hidden;
                readOnlyMessageView.Visibility = Visibility.Visible;
            }

            OfflineView.Visibility = Visibility.Hidden;
            OnlineView.Visibility = Visibility.Visible;
        }

        void chatClient_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            if (!e.Discovered)
                ShowPopup("Budy Online", e.Buddy.DisplayName + " is online");
        }        

        void PropmtDisplayMessage(object sender, RoutedEventArgs e)
        {
            readOnlyMessageView.Visibility = Visibility.Hidden;
            emptyMessageView.Visibility = Visibility.Hidden;
            writableMessageView.Visibility = Visibility.Visible;

            txtDisplayMessage.Text = chatVM.LoggedInUser.DisplayMessage;
            txtDisplayMessage.Focus();
        }

        void UpdateDisplayMessage(object sender, RoutedEventArgs e)
        {
            chatVM.LoggedInUser.DisplayMessage = txtDisplayMessage.Text;
            
            Properties.Settings.Default.DisplayMessage = txtDisplayMessage.Text;
            Properties.Settings.Default.Save();

            if (txtDisplayMessage.Text.Trim() == String.Empty)
                emptyMessageView.Visibility = Visibility.Visible;
            else
                readOnlyMessageView.Visibility = Visibility.Visible;
            
            writableMessageView.Visibility = Visibility.Hidden;
        }

        void ShowPopup(string title, string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                FancyBalloon balloon = new FancyBalloon();
                balloon.BalloonText = title;
                balloon.DataContext = message;
                trayIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 5000);
            }));
        }
    }
}
