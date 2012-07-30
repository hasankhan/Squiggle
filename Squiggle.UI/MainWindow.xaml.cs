using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Messenger;
using Squiggle.Chat;
using Squiggle.Core;
using Squiggle.Core.Chat;
using Squiggle.Core.Presence;
using Squiggle.UI.Controls;
using Squiggle.UI.Helpers;
using Squiggle.UI.Resources;
using Squiggle.UI.Settings;
using Squiggle.UI.StickyWindows;
using Squiggle.UI.ViewModel;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.Utilities.Net;
using Squiggle.Activities;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : StickyWindow
    {
        WindowState lastState;
        ClientViewModel clientViewModel;
        ChatWindowCollection chatWindows;
        ClientViewModel dummyViewModel;
        NetworkSignout autoSignout;
        ManualResetEvent clientAvailable = new ManualResetEvent(true);
        IdleStatusChanger idleStatusChanger;

        internal static MainWindow Instance { get; private set; }
        internal static PluginLoader PluginLoader { get; private set; }

        public IChatClient ChatClient { get; private set; }
        public IVoiceChat ActiveVoiceChat { get; set; }
        public bool IsVoiceChatActive
        {
            get { return ActiveVoiceChat != null; }
        }

        bool exiting;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            var pluginPath = Path.Combine(AppInfo.Location, "Plugins");
            if (!Directory.Exists(pluginPath))
                Directory.CreateDirectory(pluginPath);
            var catalog = new DirectoryCatalog(pluginPath);
            PluginLoader = new PluginLoader(catalog);

            this.Height = Properties.Settings.Default.MainWindowHeight;
            this.Width = Properties.Settings.Default.MainWindowWidth;

            this.Top = Properties.Settings.Default.MainWindowTop > 0 ? Properties.Settings.Default.MainWindowTop : System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2 - this.Height / 2;
            this.Left = Properties.Settings.Default.MainWindowLeft > 0 ? Properties.Settings.Default.MainWindowLeft : System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2 - this.Width / 2;

            TrayPopup.Instance.Enabled = SettingsProvider.Current.Settings.GeneralSettings.ShowPopups;
            AudioAlert.Instance.Enabled = SettingsProvider.Current.Settings.GeneralSettings.AudioAlerts;

            chatWindows = new ChatWindowCollection();

            chatControl.SignIn.LoginInitiated += new EventHandler<Squiggle.UI.Controls.LogInEventArgs>(ContactList_LoginInitiated);
            chatControl.ContactList.BroadcastChatStart += new EventHandler<Controls.BuddiesActionEventArgs>(ContactList_BroadcastChatStart);
            chatControl.ContactList.GroupChatStart += new EventHandler<BuddiesActionEventArgs>(ContactList_GroupChatStart);
            chatControl.ContactList.ChatStart += new EventHandler<Squiggle.UI.Controls.ChatStartEventArgs>(ContactList_StartChat);
            chatControl.ContactList.SignOut += new EventHandler(ContactList_SignOut);
            dummyViewModel = new ClientViewModel(new DummyChatClient());
            autoSignout = new NetworkSignout(this.Dispatcher, u => SignIn(u.DisplayName, u.GroupName, false, () => { }), () => SignOut(false));
            chatControl.ContactList.OpenAbout += (sender, e) => SquiggleUtility.ShowAboutDialog(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = dummyViewModel;
            this.StateChanged += new EventHandler(Window_StateChanged);

            var settings = SettingsProvider.Current.Settings;
            settings.ContactSettings.ContactGroups.FlushItems();
            SettingsProvider.Current.Save();

            string name = settings.PersonalSettings.DisplayName;
            string groupName = settings.PersonalSettings.GroupName;

            chatControl.SignIn.LoadSettings(settings);           

            if (!String.IsNullOrEmpty(name) && settings.PersonalSettings.AutoSignMeIn)
                Dispatcher.Delay(() => SignIn(name, groupName, false, () => { }),
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

        void client_ChatStarted(object sender, Squiggle.Chat.ChatStartedEventArgs e)
        {
            Dispatcher.Invoke(() => CreateChatWindow(e.Buddy, e.Chat, false));
        }

        void ContactList_StartChat(object sender, Squiggle.UI.Controls.ChatStartEventArgs e)
        {
            StartChat(e.Buddy, e.SendFile, e.Files);
        }

        void ContactList_BroadcastChatStart(object sender, Controls.BuddiesActionEventArgs e)
        {
            StartBroadcastChat(e.Buddies);
        }

        void ContactList_GroupChatStart(object sender, BuddiesActionEventArgs e)
        {
            StartGroupChat(e.Buddies);
        }

        ChatWindow StartChat(Buddy buddy, bool sendFile, params string[] filePaths)
        {
            ChatWindow window = StartChat(buddy);
            if (sendFile)
                if (filePaths == null || filePaths.Length == 0)
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
            var status = (Core.Presence.UserStatus)((System.Windows.Controls.MenuItem)e.OriginalSource).DataContext;
            clientViewModel.LoggedInUser.Status = status;
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        private void HistoryViewerMenu_Click(object sender, RoutedEventArgs e)
        {
            var viewer = new HistoryViewer();
            viewer.Owner = this;
            viewer.Show();
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
            if (ChatClient != null && ChatClient.LoggedIn)
                return;

            busyIndicator.IsBusy = true;

            Exception ex = null;

            Async.Invoke(() =>
            {
                clientAvailable.WaitOne(TimeSpan.FromSeconds(20));
                ExceptionMonster.EatTheException(() => ChatClient = CreateClient(displayName, groupName), "creating chat client", out ex);
            },
            () =>
            {
                busyIndicator.IsBusy = false;

                if (ex != null && byUser)
                {
                    MessageBox.Show(ex.Message, Translation.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CreateMonitor();
                clientViewModel = new ClientViewModel(ChatClient);
                this.DataContext = clientViewModel;
                chatControl.ChatContext = clientViewModel;
                clientViewModel.CancelUpdateCommand = new RelayCommand(CancelUpdateCommand_Execute);
                
                VisualStateManager.GoToState(chatControl, "OnlineState", true);
                autoSignout.OnSignIn(displayName, groupName);

                foreach (var window in chatWindows)
                    window.Enabled = true;

                CheckForUpdates();

                onSignIn();
            });
        }

        void CheckForUpdates()
        {
            if (!SettingsProvider.Current.Settings.GeneralSettings.CheckForUpdates)
                return;

            UpdateCheckResult result = null;
            Async.Invoke(() => result = UpdateNotifier.CheckForUpdate(SettingsProvider.Current.Settings.GeneralSettings.FirstRun),
                         () => OnUpdateCheckComplete(result));
        }

        void CancelUpdateCommand_Execute(object argument)
        {
            SettingsProvider.Current.Settings.GeneralSettings.FirstRun = DateTimeOffset.Now;
            SettingsProvider.Current.Save();
            clientViewModel.UpdateLink = null;
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

                Async.Invoke(() =>
                {
                    ExceptionMonster.EatTheException(() => ChatClient.Logout(), "loging out client");
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
                SettingsProvider.Current.Settings.ContactSettings.ContactGroups.Add(groupName);
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

            
            var properties = new BuddyProperties();
            properties.GroupName = groupName;
            properties.MachineName = Environment.MachineName;
            properties.DisplayMessage = settings.PersonalSettings.DisplayMessage;
            properties.DisplayImage = settings.PersonalSettings.DisplayImage;
            properties.EmailAddress = settings.PersonalSettings.EmailAddress;

            IChatClient client = new ChatClientFactory(settings).CreateClient();
            client.ChatStarted += new EventHandler<Squiggle.Chat.ChatStartedEventArgs>(client_ChatStarted);
            client.BuddyUpdated += new EventHandler<BuddyEventArgs>(client_BuddyUpdated);
            client.BuddyOnline += new EventHandler<BuddyOnlineEventArgs>(client_BuddyOnline);
            client.BuddyOffline += new EventHandler<BuddyEventArgs>(client_BuddyOffline);

            client.Login(displayName, properties);
            
            return client;
        }

        void client_BuddyOnline(object sender, BuddyOnlineEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!e.Discovered)
                {
                    TrayPopup.Instance.Show("Buddy Online", e.Buddy.DisplayName + " is online", _ => StartChat(e.Buddy));
                    AudioAlert.Instance.Play(AudioAlertType.BuddyOnline);
                    BlinkTrayIcon();
                }
                OnBuddyChanged(e);
            });
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
                window = chatWindows.Find(w => buddy.Equals(w.PrimaryBuddy) && !w.IsGroupChat);

            if (window == null)
            {
                window = new ChatWindow(buddy);
                window.Closed += (sender, e) => chatWindows.Remove(window);
                window.SetChatSession(chatSession ?? buddy.StartChat());
                chatWindows.Add(window);
            }
            else if (chatSession != null)
                window.SetChatSession(chatSession);

            window.Show(initiatedByUser);            

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

                foreach (Window window in Application.Current.Windows)
                    if (window != this)
                    {
                        if (window is ChatWindow)
                            ((ChatWindow)window).ForceClose();
                        else
                            window.Close();
                    }

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
            Buddy buddy = SquiggleUtility.SelectContact(Translation.Instance.ContactSelectWindow_Heading_File, this);
            if (buddy != null)
                StartChat(buddy, true, null);
        }

        private void SendMessageMenu_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Buddy> buddies = SquiggleUtility.SelectContacts(Translation.Instance.ContactSelectWindow_Heading_InstantMessage, this);
            if (buddies.Any())
            {
                Buddy buddy = buddies.First();
                ChatWindow chatWindow = StartChat(buddy);
                chatWindow.Invite(buddies.Except(new[] { buddy }));
            }
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
            var onlineBuddies = ChatClient.Buddies.Where(b => b.IsOnline);
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

        void StartGroupChat(IEnumerable<Buddy> buddies)
        {
            if (!buddies.Any())
                return;

            var chat = StartChat(buddies.FirstOrDefault());
            chat.Invite(buddies.Skip(1));
        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            var sortBy = (ContactListSortField)((MenuItem)sender).Tag;
            SettingsProvider.Current.Settings.ContactSettings.ContactListSortField = sortBy;
            SettingsProvider.Current.Save();
            UpdateSortMenu();
        }

        void UpdateSortMenu()
        {
            mnuSortByStatus.IsChecked = (SettingsProvider.Current.Settings.ContactSettings.ContactListSortField == ContactListSortField.Status);
            mnuSortByName.IsChecked = !mnuSortByStatus.IsChecked;
        }

        private void GroupMenu_Click(object sender, RoutedEventArgs e)
        {
            SettingsProvider.Current.Settings.ContactSettings.GroupContacts = !SettingsProvider.Current.Settings.ContactSettings.GroupContacts;
            SettingsProvider.Current.Save();
            UpdateGroupMenu();
        }

        void UpdateGroupMenu()
        {
            mnuGroupBuddies.IsChecked = SettingsProvider.Current.Settings.ContactSettings.GroupContacts;
        }

        void OnUpdateCheckComplete(UpdateCheckResult result)
        {
            if (result != null && result.IsUpdated)
                clientViewModel.UpdateLink = result.UpdateLink;
        }

        void BlinkTrayIcon()
        {
            ((Storyboard)this.FindResource("blinkTrayIcon")).Begin();
        }

        private void StickyWindow_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MainWindowTop = Top;
            Properties.Settings.Default.MainWindowLeft = Left;

            Properties.Settings.Default.Save();
        }
    }
}
