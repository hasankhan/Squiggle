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
using StackOverflowClient;
using System.Windows.Controls.Primitives;
using System.Net;
using System.Windows.Threading;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChatClientControl.xaml
    /// </summary>
    public partial class ChatClientControl : UserControl
    {
        WindowState lastState;
        public Window MainWindow {get; set; }

        public ChatClientControl()
        {
           InitializeComponent();

            if (!String.IsNullOrEmpty(Properties.Settings.Default.DisplayName))
                SignIn(Properties.Settings.Default.DisplayName);
        }

        IChatClient chatClient;
        ClientViewModel clientViewModel;
        UserActivityMonitor activityMonitor;
        UserStatus lastStatus;

        void OnCredentialsVerfied(object sender, Squiggle.UI.Controls.LogInEventArgs e)
        {
            SignIn(e.UserName);
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            CreateChatWindow(e.Buddy, e.Message, e.Chat, WindowState.Minimized);
        }

        static void CreateChatWindow(Buddy buddy, string message, IChat session, WindowState state)
        {
            ChatWindow window = new ChatWindow(buddy, message);
            window.Title = buddy.DisplayName;
            window.DataContext = session;
            window.WindowState = state;
            window.Show();
        }   

        private void SignIn(string displayName)
        {
            InitializeClient(displayName);
            clientViewModel = new ClientViewModel(chatClient);
            this.DataContext = clientViewModel;
            Online.ChatContext = clientViewModel;

            VisualStateManager.GoToState(this, "OnlineState", true);
        }        

        void chatClient_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            if (!e.Discovered)
                ShowPopup("Budy Online", e.Buddy.DisplayName + " is online");
        }

        void OnChatStart(object sender, Squiggle.UI.Controls.ChatStartEventArgs e)
        {
            CreateChatWindow(e.User, string.Empty, e.User.StartChat(), WindowState.Normal);
        }

        void ShowPopup(string title, string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                FancyBalloon balloon = new FancyBalloon();
                balloon.BalloonText = title;
                balloon.DataContext = message;
                Hardcodet.Wpf.TaskbarNotification.TaskbarIcon icon = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon();
                icon.Visibility = Visibility.Hidden;
                icon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 5000);
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
            int chatPort = NetworkUtility.GetFreePort();            
            IPAddress localIP = NetworkUtility.GetLocalIPAddress();
            TimeSpan keepAliveTimeout = 2.Seconds();
            var chatEndPoint = new IPEndPoint(localIP, chatPort);
            var presenceEndPoint = new IPEndPoint(IPAddress.Parse("224.10.11.12"), 9999);
            var client = new ChatClient(chatEndPoint, presenceEndPoint, keepAliveTimeout);
            client.Login(displayName);
            client.ChatStarted += new EventHandler<ChatStartedEventArgs>(chatClient_ChatStarted);
            client.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(chatClient_BuddyOnline);
            return client;
        }

        private void RestoreWindow()
        {
            MainWindow.Visibility = System.Windows.Visibility.Visible;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(delegate()
                {
                    MainWindow.WindowState = lastState;
                    MainWindow.Activate();
                }));
        }

        private void trayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Visibility == Visibility.Visible)
                MainWindow.Hide();
            else
                RestoreWindow();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (MainWindow.WindowState == System.Windows.WindowState.Minimized)
                MainWindow.Visibility = System.Windows.Visibility.Hidden;
            else
                lastState = MainWindow.WindowState;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.StateChanged += new EventHandler(Window_StateChanged);
        }

    }
}
