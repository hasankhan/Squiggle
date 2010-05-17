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
        IChatClient chatClient;
        ClientViewModel clientViewModel;
        UserActivityMonitor activityMonitor;
        UserStatus lastStatus;

        public MainWindow()
        {
            InitializeComponent();

            if (!String.IsNullOrEmpty(Properties.Settings.Default.DisplayName))
                SignIn(Properties.Settings.Default.DisplayName);
            
        }

        void OnCredentialsVerfied(object sender, Squiggle.UI.Controls.LogInEventArgs e)
        {
            SignIn(e.UserName);
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            CreateChatWindow(e.Buddy, e.Message, e.Chat);
        }

        static void CreateChatWindow(Buddy buddy, string message, IChat session)
        {
            ChatWindow window = new ChatWindow(buddy, message);
            window.Title = buddy.DisplayName;
            window.DataContext = session;
            window.Topmost = true;
            window.Show();
        }   

        private void SignIn(string displayName)
        {
            InitializeClient(displayName);
            clientViewModel = new ClientViewModel(chatClient);
            this.DataContext = clientViewModel;
            Online.ChatContext = clientViewModel;

            //VisualStateManager.GoToState(this, "OnlineState", true);
            Online.Opacity = 1;
            Offline.Opacity = 0;
        }        

        void chatClient_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            if (!e.Discovered)
                ShowPopup("Budy Online", e.Buddy.DisplayName + " is online");
        }

        void OnChatStart(object sender, Squiggle.UI.Controls.ChatStartEventArgs e)
        {
            CreateChatWindow(e.User, string.Empty, e.User.StartChat());
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

        void InitializeClient(string displayName)
        {
            chatClient = CreateClient(displayName);
            CreateMonitor();
        }

        void CreateMonitor()
        {
            if (activityMonitor != null)
                activityMonitor.Dispose();
            activityMonitor = new UserActivityMonitor(5.Minutes());
            activityMonitor.Idle += (sender, e) =>
            {
                if (chatClient.LoggedIn)
                {
                    lastStatus = chatClient.CurrentUser.Status;
                    chatClient.CurrentUser.Status = UserStatus.Idle;
                }
            };
            activityMonitor.Active += (sender, e) =>
            {
                if (chatClient.LoggedIn)
                    chatClient.CurrentUser.Status = lastStatus;
            };
            activityMonitor.Start();
        }

        IChatClient CreateClient(string displayName)
        {
            int presencePort = 9999;
            int chatPort = NetworkUtility.GetFreePort();            
            IPAddress localIP = NetworkUtility.GetLocalIPAddress();
            TimeSpan keepAliveTimeout = 2.Seconds();
            var localEndPoint = new IPEndPoint(localIP, chatPort);
            var client = new ChatClient(localEndPoint, presencePort, keepAliveTimeout);
            client.Login(displayName);
            client.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            client.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            return client;
        }
    }
}
