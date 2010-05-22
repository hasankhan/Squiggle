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
using System.Windows.Threading;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowState lastState;
        IChatClient chatClient;
        ClientViewModel clientViewModel;
        UserActivityMonitor activityMonitor;
        UserStatus lastStatus;
        IPAddress presenceAddress = IPAddress.Parse("224.10.11.12");

        public MainWindow()
        {
           InitializeComponent();

           chatControl.SignIn.CredentialsVerfied += new EventHandler<Squiggle.UI.Controls.LogInEventArgs>(OnCredentialsVerfied);
           chatControl.UserInfo.ChatStart += new EventHandler<Squiggle.UI.Controls.ChatStartEventArgs>(OnStartChat);

            if (!String.IsNullOrEmpty(Properties.Settings.Default.DisplayName))
                SignIn(Properties.Settings.Default.DisplayName);
        }

        void OnCredentialsVerfied(object sender, Squiggle.UI.Controls.LogInEventArgs e)
        {
            SignIn(e.UserName);
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            CreateChatWindow(e.Buddy, e.Message, e.Chat, false);
        }

        static void CreateChatWindow(Buddy buddy, string message, IChat session, bool focused)
        {
            ChatWindow window = new ChatWindow(buddy, message);
            window.Title = buddy.DisplayName;
            window.DataContext = session;
            if (!focused)
                window.WindowState = WindowState.Minimized;
            window.Show();
            if (focused)
                window.Activate();
        }   

        private void SignIn(string displayName)
        {
            InitializeClient(displayName);
            clientViewModel = new ClientViewModel(chatClient);
            this.DataContext = clientViewModel;
            chatControl.ChatContext = clientViewModel;

            VisualStateManager.GoToState(chatControl, "OnlineState", true);
        }        

        void chatClient_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            if (!e.Discovered)
                TrayPopup.Show("Budy Online", e.Buddy.DisplayName + " is online", _=> StartChat(e.Buddy));
        }

        void OnStartChat(object sender, Squiggle.UI.Controls.ChatStartEventArgs e)
        {
            StartChat(e.User);
        }

        static void StartChat(Buddy buddy)
        {
            CreateChatWindow(buddy, String.Empty, buddy.StartChat(), true);
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
            int chatPort = NetworkUtility.GetFreePort();            
            IPAddress localIP = NetworkUtility.GetLocalIPAddress();
            TimeSpan keepAliveTimeout = 2.Seconds();
            var chatEndPoint = new IPEndPoint(localIP, chatPort);
            var presenceEndPoint = new IPEndPoint(presenceAddress, 9999);
            var client = new ChatClient(chatEndPoint, presenceEndPoint, keepAliveTimeout);
            client.Login(displayName);
            client.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            client.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            return client;
        }

        private void RestoreWindow()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(delegate()
                {
                    this.WindowState = lastState;
                    this.Activate();
                }));
        }

        private void trayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
                this.Hide();
            else
                RestoreWindow();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
                this.Visibility = System.Windows.Visibility.Hidden;
            else
                lastState = this.WindowState;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.StateChanged += new EventHandler(Window_StateChanged);
        }   
    }
}
