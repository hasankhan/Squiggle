using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Messenger;
using Squiggle.Chat;
using Squiggle.UI.Settings;
using Squiggle.UI.Helpers;
using Squiggle.UI.ViewModel;
using System.Windows.Controls;
using System.Diagnostics;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowState lastState;
        ClientViewModel clientViewModel;
        ChatWindowCollection chatWindows;
        ClientViewModel dummyViewModel;
        NetworkSignout autoSignout;
        ManualResetEvent clientAvailable = new ManualResetEvent(true);
        IdleStatusChanger idleStatusChanger;

        public static MainWindow Instance { get; private set; }
        public IChatClient ChatClient { get; private set; }

        bool exiting;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
           
            this.Height = Properties.Settings.Default.MainWindowHeight;
            this.Width = Properties.Settings.Default.MainWindowWidth;

            this.Top = Properties.Settings.Default.MainWindowTop > 0 ? Properties.Settings.Default.MainWindowTop : System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2 - this.Height / 2;
            this.Left = Properties.Settings.Default.MainWindowLeft > 0 ? Properties.Settings.Default.MainWindowLeft : System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width /2 - this.Width / 2;

            TrayPopup.Instance.Enabled = SettingsProvider.Current.Settings.GeneralSettings.ShowPopups;
            AudioAlert.Instance.Enabled = SettingsProvider.Current.Settings.GeneralSettings.AudioAlerts;

            chatWindows = new ChatWindowCollection();

            chatControl.SignIn.LoginInitiated += new EventHandler<Squiggle.UI.Controls.LogInEventArgs>(ContactList_LoginInitiated);
            chatControl.ContactList.BroadcastChatStart += new EventHandler<Controls.BroadcastChatStartEventArgs>(ContactList_BroadcastChatStart);
            chatControl.ContactList.ChatStart += new EventHandler<Squiggle.UI.Controls.ChatStartEventArgs>(ContactList_StartChat);
            chatControl.ContactList.SignOut += new EventHandler(ContactList_SignOut);
            dummyViewModel = new ClientViewModel(new DummyChatClient());
            autoSignout = new NetworkSignout(u => SignIn(u.DisplayName, u.GroupName, false, () => { }), () => SignOut(false));
            chatControl.ContactList.OpenAbout += (sender, e) => SquiggleUtility.ShowAboutDialog(this);
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = dummyViewModel;
            this.StateChanged += new EventHandler(Window_StateChanged);

            var settings = SettingsProvider.Current.Settings;
            settings.GeneralSettings.ContactGroups.FlushItems();
            SettingsProvider.Current.Save();

            string name = settings.PersonalSettings.DisplayName;
            string groupName = settings.PersonalSettings.GroupName;

            chatControl.SignIn.chkAutoSignIn.IsChecked = settings.PersonalSettings.AutoSignMeIn;
            chatControl.SignIn.chkRememberName.IsChecked = settings.PersonalSettings.RememberMe;
            chatControl.SignIn.SetDisplayName(name);
            chatControl.SignIn.SetGroupName(groupName);
            chatControl.SignIn.LoadGroups(settings.GeneralSettings.ContactGroups);

            if (!String.IsNullOrEmpty(name) && settings.PersonalSettings.AutoSignMeIn)
                Async.Invoke(() => SignIn(name, groupName, false, () => { }),
                             TimeSpan.FromSeconds(5));
            else if (!String.IsNullOrEmpty(name))
                    chatControl.SignIn.chkRememberName.IsChecked = true;
            
            UpdateSortMenu();
            UpdateGroupMenu();

            if (App.RunInBackground)
                this.Hide();
        }

        void ContactList_LoginInitiated(object sender, Squiggle.UI.Controls.LogInEventArgs e)
        {
            SignIn(e.UserName, e.GroupName, true, () => { });
        }

        void client_ChatStarted(object sender, ChatStartedEventArgs e)
        {
            Dispatcher.Invoke(()=>CreateChatWindow(e.Buddy, e.Chat, false));
        }

        void ContactList_StartChat(object sender, Squiggle.UI.Controls.ChatStartEventArgs e)
        {
            StartChat(e.Buddy, e.SendFile, e.Files);
        }

        void ContactList_BroadcastChatStart(object sender, Controls.BroadcastChatStartEventArgs e)
        {
            StartBroadcastChat(e.Buddies);
        }

        ChatWindow StartChat(Buddy buddy, bool sendFile, params string[] filePaths)
        {
            ChatWindow window = StartChat(buddy);
            if (sendFile)
                if (filePaths == null)
                    window.SendFile();
                else
                    window.SendFiles(filePaths);

            return window;
        }

        ChatWindow StartChat(Buddy buddy)
        {
            var window = CreateChatWindow(buddy, null, true);
            return window;
        }

        private void trayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            ToggleMainWindow();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Minimized)
                lastState = this.WindowState;
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
            Quit();
        }

        public void Quit()
        {
            exiting = true;
            Close();            
        }

        void ContactList_SignOut(object sender, EventArgs e)
        {
            SignOut(true);
        }

        private void SignOutMenu_Click(object sender, RoutedEventArgs e)
        {
            SignOut(true);
        }   

        void SignIn(string displayName, string groupName, bool byUser, Action onSignIn)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (ChatClient != null && ChatClient.LoggedIn)
                    return;

                clientAvailable.WaitOne(TimeSpan.FromSeconds(20));

                foreach (var window in chatWindows)
                    window.Enabled = true;                

                try
                {
                    ChatClient = CreateClient(displayName, groupName);
                }
                catch (Exception ex)
                {
                    if (byUser)
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CreateMonitor();
                clientViewModel = new ClientViewModel(ChatClient);
                this.DataContext = clientViewModel;
                chatControl.ChatContext = clientViewModel;

                VisualStateManager.GoToState(chatControl, "OnlineState", true);
                autoSignout.OnSignIn(displayName, groupName);

                onSignIn();
            }));
        }
        
        void SignOut(bool byUser)
        {
            if (ChatClient == null || !ChatClient.LoggedIn)
                return;

            clientAvailable.Reset();

            Dispatcher.Invoke((Action)(() =>
            {
                foreach (var window in chatWindows)
                    window.DestroySession();

                DestroyMonitor();

                chatControl.SignIn.SetDisplayName(ChatClient.CurrentUser.DisplayName);

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        ChatClient.Logout();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                    clientAvailable.Set();
                });

                chatControl.ContactList.ChatContext = null;
                clientViewModel = null;
                this.DataContext = dummyViewModel;
                VisualStateManager.GoToState(chatControl, "OfflineState", true);

                if (byUser)
                    autoSignout.OnSignOut();
            }));
        }

        static void AddGroupName(string groupName)
        {
            if (!groupName.Equals(BuddyProperties.DefaultGroupName))
            {
                SettingsProvider.Current.Settings.GeneralSettings.ContactGroups.Add(groupName);
                SettingsProvider.Current.Save();
            }
        }

        void ToggleMainWindow()
        {
            Dispatcher.Invoke(() =>
            {
                if (this.Visibility == Visibility.Visible)
                    this.Hide();
                else
                    RestoreWindow();
            });
        }

        void CreateMonitor()
        {
            TimeSpan timeout = SettingsProvider.Current.Settings.PersonalSettings.IdleTimeout.Minutes();
            idleStatusChanger = new IdleStatusChanger(ChatClient, timeout);
        }

        private void DestroyMonitor()
        {
            idleStatusChanger.Dispose();
            idleStatusChanger = null;
        }        

        IChatClient CreateClient(string displayName, string groupName)
        {
            SettingsProvider.Current.Load(); // reload settings
            var settings = SettingsProvider.Current.Settings;

            int chatPort = settings.ConnectionSettings.ChatPort;
            if (String.IsNullOrEmpty(settings.ConnectionSettings.BindToIP))
                throw new OperationCanceledException("You are not on a network. Please make sure your network card is enabled.");

            var localIP = IPAddress.Parse(settings.ConnectionSettings.BindToIP);
            TimeSpan keepAliveTimeout = settings.ConnectionSettings.KeepAliveTime.Seconds();
            var presenceAddress = IPAddress.Parse(settings.ConnectionSettings.PresenceAddress);
            int presencePort = settings.ConnectionSettings.PresencePort;

            var chatEndPoint = new IPEndPoint(localIP, chatPort);
            if (!NetworkUtility.IsEndPointFree(chatEndPoint))
                chatEndPoint.Port = NetworkUtility.GetFreePort();

            var presenceEndPoint = new IPEndPoint(presenceAddress, presencePort);
            string clientID = settings.ConnectionSettings.ClientID;

            var client = new ChatClient(new ChatEndPoint(clientID, chatEndPoint), presenceEndPoint, keepAliveTimeout);

            var properties = new BuddyProperties();
            properties.GroupName = groupName;
            properties.MachineName = Environment.MachineName;
            properties.DisplayMessage = settings.PersonalSettings.DisplayMessage;
            client.Login(displayName, properties);
            client.ChatStarted += new EventHandler<ChatStartedEventArgs>(client_ChatStarted);
            client.BuddyUpdated += new EventHandler<BuddyEventArgs>(client_BuddyUpdated);
            client.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(client_BuddyOnline);
            client.BuddyOffline += new EventHandler<BuddyEventArgs>(client_BuddyOffline);
            return client;
        }

        void client_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            if (!e.Discovered)
            {
                TrayPopup.Instance.Show("Buddy Online", e.Buddy.DisplayName + " is online", _ => StartChat(e.Buddy));
                AudioAlert.Instance.Play(AudioAlertType.BuddyOnline);
            }
            OnBuddyChanged(e);
        }

        void client_BuddyOffline(object sender, BuddyEventArgs e)
        {
            AudioAlert.Instance.Play(AudioAlertType.BuddyOffline);
            OnBuddyChanged(e);
        }

        void client_BuddyUpdated(object sender, BuddyEventArgs e)
        {
            OnBuddyChanged(e);
        }

        void OnBuddyChanged(BuddyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AddGroupName(e.Buddy.Properties.GroupName);
                chatControl.ContactList.Refresh();
            });
        }  

        public void RestoreWindow()
        {
            this.Show();
            this.WindowState = lastState;
            this.Activate();
        }

        ChatWindow CreateChatWindow(Buddy buddy, IChat chatSession, bool initiatedByUser)
        {
            ChatWindow window = null;
            
            if (chatSession == null || !chatSession.IsGroupChat)
                window = chatWindows.Find(w => w.Buddies.Contains(buddy) && !w.IsGroupChat);

            if (window == null)
            {
                window = new ChatWindow(buddy);
                window.Closed += (sender, e) => chatWindows.Remove(window);
                window.SetChatSession(chatSession ?? buddy.StartChat());
                chatWindows.Add(window);
                if (!initiatedByUser)
                    window.WindowState = WindowState.Minimized;
                window.Show();
            }
            else if (chatSession != null)
                window.SetChatSession(chatSession);

            if (initiatedByUser)
                window.Restore();

            return window;
        }      

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!exiting && SettingsProvider.Current.Settings.GeneralSettings.HideToSystemTray)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Hidden;

                trayIcon.Dispose();
                autoSignout.Dispose();

                foreach (var window in chatWindows.ToList())
                    window.Close();

                Properties.Settings.Default.MainWindowTop = Top;
                Properties.Settings.Default.MainWindowLeft = Left;

                SignOut(true);
                clientAvailable.WaitOne();
            }
        }

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenReceivedFilesMenu_Click(object sender, RoutedEventArgs e)
        {
            SquiggleUtility.OpenDownloadsFolder();
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            SquiggleUtility.ShowSettingsDialog(this);
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            SquiggleUtility.ShowAboutDialog(this);
        }

        private void SendFileMenu_Click(object sender, RoutedEventArgs e)
        {
            Buddy buddy = SquiggleUtility.SelectContact("Send a file", this);
            if (buddy != null)
                StartChat(buddy, true, null);
        }

        private void SendMessageMenu_Click(object sender, RoutedEventArgs e)
        {
            Buddy buddy = SquiggleUtility.SelectContact("Send an instant message", this);
            if (buddy != null)
                StartChat(buddy);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Normal)
            {
                Properties.Settings.Default.MainWindowHeight = this.Height;
                Properties.Settings.Default.MainWindowWidth = this.Width;
                Properties.Settings.Default.Save();
            }
        }

        private void SendBroadcastMessageMenu_Click(object sender, RoutedEventArgs e)
        {
            StartBroadcastChat();
        }

        void StartBroadcastChat()
        {
            var onlineBuddies = ChatClient.Buddies.Where(b => b.Status != UserStatus.Offline);
            if (onlineBuddies.Any())
                StartBroadcastChat(onlineBuddies);
        }

        void StartBroadcastChat(IEnumerable<Buddy> buddies)
        {
            var chatSessions = buddies.Select(b => b.StartChat()).ToList();
            var groupChat = new BroadcastChat(chatSessions);
            CreateChatWindow(groupChat.Buddies.First(), groupChat, true);
            ChatClient.BuddyOnline += (s, b) => groupChat.AddSession(b.Buddy.StartChat());
            ChatClient.BuddyOffline += (s, b) =>
            {
                var session = groupChat.ChatSessions.FirstOrDefault(c => c.Buddies.Contains(b.Buddy) && !c.IsGroupChat);
                groupChat.RemoveSession(session);
            };
        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            string sortBy = ((MenuItem)sender).Tag.ToString();
            SettingsProvider.Current.Settings.GeneralSettings.ContactListSortField = sortBy;
            SettingsProvider.Current.Save();
            UpdateSortMenu();
        }

        void UpdateSortMenu()
        {
            mnuSortByStatus.IsChecked = (SettingsProvider.Current.Settings.GeneralSettings.ContactListSortField == "Status");
            mnuSortByName.IsChecked = !mnuSortByStatus.IsChecked;
        }

        private void GroupMenu_Click(object sender, RoutedEventArgs e)
        {
            SettingsProvider.Current.Settings.GeneralSettings.GroupContacts = !SettingsProvider.Current.Settings.GeneralSettings.GroupContacts;
            SettingsProvider.Current.Save();
            UpdateGroupMenu();
        }

        void UpdateGroupMenu()
        {
            mnuGroupBuddies.IsChecked = SettingsProvider.Current.Settings.GeneralSettings.GroupContacts;
        }
    }
}
