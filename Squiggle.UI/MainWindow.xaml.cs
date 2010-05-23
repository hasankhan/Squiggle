using System;
using System.Collections.Generic;
using System.Windows;
using Squiggle.Chat;
using System.Net;
using System.Linq;
using System.Windows.Threading;
using Squiggle.UI.Settings;

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
        Dictionary<Buddy, ChatWindow> chatWindows;

        public MainWindow()
        {
           InitializeComponent();

           chatWindows = new Dictionary<Buddy, ChatWindow>();

           chatControl.SignIn.CredentialsVerfied += new EventHandler<Squiggle.UI.Controls.LogInEventArgs>(OnCredentialsVerfied);
           chatControl.ContactList.ChatStart += new EventHandler<Squiggle.UI.Controls.ChatStartEventArgs>(OnStartChat);           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string name = SettingsProvider.Current.Settings.PersonalSettings.DisplayName;
            chatControl.SignIn.txtdisplayName.Text = name;
            chatControl.SignIn.txtdisplayName.SelectAll();
            if (!String.IsNullOrEmpty(name))
                chatControl.SignIn.chkRememberName.IsChecked = true;
            this.StateChanged += new EventHandler(Window_StateChanged);
        }   

        void OnCredentialsVerfied(object sender, Squiggle.UI.Controls.LogInEventArgs e)
        {
            SignIn(e.UserName);
        }

        void chatClient_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            CreateChatWindow(e.Buddy, e.Message, e.Chat, false);
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

        void StartChat(Buddy buddy)
        {
            CreateChatWindow(buddy, String.Empty, buddy.StartChat(), true);
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

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var window in chatWindows.Values.ToList())
                window.Close();
        }

        private void StatusMenu_Click(object sender, RoutedEventArgs e)
        {
            var status = (UserStatus)((System.Windows.Controls.MenuItem)e.OriginalSource).DataContext;
            clientViewModel.LoggedInUser.Status = status;
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        private void QuiteMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SignOutMenu_Click(object sender, RoutedEventArgs e)
        {
            SignOut();
        }   

        private void SignIn(string displayName)
        {
            try
            {
                chatClient = CreateClient(displayName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            signoutMenu.IsEnabled = statusMenu.IsEnabled = true;
            CreateMonitor();
            clientViewModel = new ClientViewModel(chatClient);
            this.DataContext = clientViewModel;
            chatControl.ChatContext = clientViewModel;

            VisualStateManager.GoToState(chatControl, "OnlineState", true);
        }

        private void SignOut()
        {
            signoutMenu.IsEnabled = statusMenu.IsEnabled = false;
            DestroyMonitor();
            chatClient.Logout();
            clientViewModel = null;
            this.DataContext = null;
            VisualStateManager.GoToState(chatControl, "OfflineState", true);
        }

        void CreateMonitor()
        {
            TimeSpan timeout = SettingsProvider.Current.Settings.PersonalSettings.IdleTimeout.Minutes();
            if (activityMonitor != null)
                activityMonitor.Dispose();
            activityMonitor = new UserActivityMonitor(timeout);
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

        private void DestroyMonitor()
        {
            if (activityMonitor != null)
            {
                activityMonitor.Dispose();
                activityMonitor = null;
            }
        }        

        IChatClient CreateClient(string displayName)
        {
            var settings = SettingsProvider.Current.Settings;

            int chatPort = settings.ConnectionSettings.ChatPort;
            if (String.IsNullOrEmpty(settings.ConnectionSettings.BindToIP))
                throw new OperationCanceledException("You are not on a network. Please make sure your network card is enabled.");

            IPAddress localIP = IPAddress.Parse(settings.ConnectionSettings.BindToIP);
            TimeSpan keepAliveTimeout = settings.ConnectionSettings.KeepAliveTime.Seconds();
            IPAddress presenceAddress = IPAddress.Parse(settings.ConnectionSettings.PresenceAddress);
            int presencePort = settings.ConnectionSettings.PresencePort;

            var chatEndPoint = new IPEndPoint(localIP, chatPort);
            if (!NetworkUtility.IsEndPointFree(chatEndPoint))
                chatEndPoint.Port = NetworkUtility.GetFreePort();

            var presenceEndPoint = new IPEndPoint(presenceAddress, presencePort);

            ChatClient client = new ChatClient(chatEndPoint, presenceEndPoint, keepAliveTimeout);
            
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

        void CreateChatWindow(Buddy buddy, string message, IChat session, bool focused)
        {
            ChatWindow window;

            if (!chatWindows.TryGetValue(buddy, out window))
            {
                window = new ChatWindow(buddy, message);
                window.Title = buddy.DisplayName;
                window.Closed += (sender, e) => chatWindows.Remove(buddy);
                chatWindows.Add(buddy, window);
            }
            window.DataContext = session;
            if (!focused)
                window.WindowState = WindowState.Minimized;
            window.Show();
            if (focused)
                window.Activate();
        }
    }
}
