using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Squiggle.Client;
using Squiggle.Core.Presence;
using Squiggle.UI.Components;
using Squiggle.UI.Controls;
using Squiggle.UI.Factories;
using Squiggle.UI.Helpers;
using Squiggle.UI.Helpers.Collections;
using Squiggle.UI.Resources;
using Squiggle.UI.Settings;
using Squiggle.UI.StickyWindow;
using Squiggle.UI.ViewModel;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;
using Squiggle.Plugins;
using System.Net;
using Squiggle.Plugins.Authentication;
using Squiggle.UI.Components.Authentication;

namespace Squiggle.UI.Windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : StickyWindowBase, IMainWindow
    {
        WindowState lastState;
        ClientViewModel clientViewModel;
        ChatWindowCollection chatWindows;
        NetworkSignout autoSignout;
        TimeoutSignal clientAvailable = new TimeoutSignal(5.Seconds());
        IdleStatusChanger idleStatusChanger;
        SquiggleContext context;

        bool exiting;

        public MainWindow()
        {
            InitializeComponent();
        }       

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chatWindows = new ChatWindowCollection();

            LoadPosition();

            var settingsProvider = SettingsProvider.Current;
            SquiggleSettings settings = settingsProvider.Settings;
            settings.ContactSettings.ContactGroups.FlushItems();
            settingsProvider.Save();

            context = new SquiggleContextFactory(new PluginLoaderFactory(), this, settings.ConnectionSettings.ClientID).CreateInstance();
            context.PluginLoader.LoadAll(context);

            SetupControls();

            TrayPopup.Instance.Enabled = SettingsProvider.Current.Settings.GeneralSettings.ShowPopups;
            AudioAlert.Instance.Enabled = SettingsProvider.Current.Settings.GeneralSettings.AudioAlerts;

            chatControl.SignIn.LoadSettings(settings);

            var client = context.ChatClient;
            client.EnableLogging = settings.GeneralSettings.EnableStatusLogging;
            client.ChatStarted += client_ChatStarted;
            client.BuddyUpdated += client_BuddyUpdated;
            client.BuddyOnline += client_BuddyOnline;
            client.BuddyOffline += client_BuddyOffline;

            clientViewModel = new ClientViewModel(client);
            clientViewModel.CancelUpdateCommand = new RelayCommand<object>(CancelUpdateCommand_Execute);
            DataContext = chatControl.ChatContext = clientViewModel;
           
            autoSignout = new NetworkSignout(this.Dispatcher, options => SignIn(options, byUser: false), () => SignOut(byUser: false));

            if (settings.PersonalSettings.RememberMe && settings.PersonalSettings.AutoSignMeIn)
            {
                var signInOptions = new SignInOptions()
                {
                    Username = settings.PersonalSettings.Username,
                    Password = settings.PersonalSettings.Password,
                    Domain = settings.PersonalSettings.Domain
                };
                Dispatcher.Delay(() => SignIn(signInOptions, false), 5.Seconds());
            }
            else if (settings.PersonalSettings.RememberMe)
                chatControl.SignIn.RememberMe = true;
        }

        private void SignInControl_LoginInitiated(object sender, Squiggle.UI.Controls.LogInEventArgs e)
        {
            var signInOptions = new SignInOptions()
            {
                Username = e.Username,
                Password = e.Password,
                Domain = e.Domain,
                DisplayName = e.DisplayName,
                GroupName = e.GroupName
            };
            SignIn(signInOptions, true);
        }

        private void client_ChatStarted(object sender, Squiggle.Client.ChatStartedEventArgs e)
        {
            Dispatcher.Invoke(() => CreateChatWindow(e.Buddy, e.Chat, false));
        }

        private void ContactList_StartChat(object sender, Squiggle.UI.Controls.ChatStartEventArgs e)
        {
            StartChat(e.Buddy, e.SendFile, e.Files);
        }

        private void ContactList_BroadcastChatStart(object sender, Controls.BuddiesActionEventArgs e)
        {
            StartBroadcastChat(e.Buddies);
        }

        private void ContactList_GroupChatStart(object sender, BuddiesActionEventArgs e)
        {
            StartGroupChat(e.Buddies);
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
            var viewer = new HistoryViewer(context);
            viewer.Owner = this;
            viewer.Show();
        }

        private void QuiteMenu_Click(object sender, RoutedEventArgs e)
        {
            Quit();
        }

        private void ContactList_SignOut(object sender, EventArgs e)
        {
            SignOut(true);
        }

        private void SignOutMenu_Click(object sender, RoutedEventArgs e)
        {
            SignOut(true);
        }

        private void CancelUpdateCommand_Execute(object argument)
        {
            SettingsProvider.Current.Settings.GeneralSettings.FirstRun = DateTimeOffset.Now;
            SettingsProvider.Current.Save();
            clientViewModel.UpdateLink = null;
        }

        private void client_BuddyOnline(object sender, BuddyOnlineEventArgs e)
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

        private void client_BuddyOffline(object sender, BuddyEventArgs e)
        {
            AudioAlert.Instance.Play(AudioAlertType.BuddyOffline);
            OnBuddyChanged(e);
        }

        private void client_BuddyUpdated(object sender, BuddyEventArgs e)
        {
            OnBuddyChanged(e);
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
                clientAvailable.Wait();
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
                IChatWindow chatWindow = StartChat(buddy);
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

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MainWindowTop = Top;
            Properties.Settings.Default.MainWindowLeft = Left;

            Properties.Settings.Default.Save();
        }

        private void SendBroadcastMessageMenu_Click(object sender, RoutedEventArgs e)
        {
            StartBroadcastChat();
        }

        private void SortMenu_Click(object sender, RoutedEventArgs e)
        {
            var sortBy = (ContactListSortField)((MenuItem)sender).Tag;
            SettingsProvider.Current.Settings.ContactSettings.ContactListSortField = sortBy;
            SettingsProvider.Current.Save();
            UpdateSortMenu();
        }

        private void GroupMenu_Click(object sender, RoutedEventArgs e)
        {
            SettingsProvider.Current.Settings.ContactSettings.GroupContacts = !SettingsProvider.Current.Settings.ContactSettings.GroupContacts;
            SettingsProvider.Current.Save();
            UpdateGroupMenu();
        }

        public IChatWindow StartChat(IBuddy buddy)
        {
            var window = CreateChatWindow(buddy, null, true);
            return window;
        }

        public void ToggleMainWindow()
        {
            Dispatcher.Invoke(() =>
            {
                if (this.Visibility == Visibility.Visible)
                    this.Hide();
                else
                    RestoreWindow();
            });
        }

        public void RestoreWindow()
        {
            this.Show();
            this.WindowState = lastState;
            this.Activate();
        }

        public void StartBroadcastChat()
        {
            var onlineBuddies = context.ChatClient.Buddies.Where(b => b.IsOnline());
            if (onlineBuddies.Any())
                StartBroadcastChat(onlineBuddies);
        }

        public void StartBroadcastChat(IEnumerable<IBuddy> buddies)
        {
            var chatSessions = buddies.Select(b => context.ChatClient.StartChat(b)).ToList();
            var groupChat = new BroadcastChat(chatSessions);
            CreateChatWindow(groupChat.Buddies.First(), groupChat, true);
            context.ChatClient.BuddyOnline += (s, e) => groupChat.AddSession(context.ChatClient.StartChat(e.Buddy));
            context.ChatClient.BuddyOffline += (s, e) =>
            {
                var session = groupChat.ChatSessions.FirstOrDefault(c => c.Buddies.Contains(e.Buddy) && !c.IsGroupChat);
                groupChat.RemoveSession(session);
            };
        }

        public void StartGroupChat(IEnumerable<IBuddy> buddies)
        {
            if (!buddies.Any())
                return;

            var chat = StartChat(buddies.FirstOrDefault());
            chat.Invite(buddies.Skip(1));
        }

        public void BlinkTrayIcon()
        {
            ((Storyboard)this.FindResource("blinkTrayIcon")).Begin();
        }

        public void Quit()
        {
            exiting = true;
            Close();
        }

        public void SignOut()
        {
            SignOut(true);
        }

        public void SignIn(SignInOptions options)
        {
            SignIn(options, true);
        }

        void SignIn(SignInOptions options, bool byUser)
        {
            if (context.ChatClient.IsLoggedIn)
                return;

            busyIndicator.IsBusy = true;

            Exception ex = null;

            Async.Invoke(() =>
            {
                clientAvailable.Wait();
                SettingsProvider.Current.Load();
                var loginHelper = new LoginHelper(context.PluginLoader.AuthenticationProvider, SettingsProvider.Current.Settings, context.ChatClient); 
                ExceptionMonster.EatTheException(() => loginHelper.Login(options), "logging in", out ex);
            },
            () =>
            {
                busyIndicator.IsBusy = false;

                if (ex != null && byUser)
                {
                    MessageBox.Show(ex.Message, Translation.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OnSignIn(options);
            });
        }

        void OnBuddyChanged(BuddyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AddGroupName(e.Buddy.Properties.GroupName);
                chatControl.ContactList.Refresh();
            });
        }

        void OnUpdateCheckComplete(UpdateCheckResult result)
        {
            if (result != null && result.IsUpdated)
                clientViewModel.UpdateLink = result.UpdateLink;
        }

        void OnSignout(bool byUser)
        {
            chatControl.ContactList.ChatContext = null;
            VisualStateManager.GoToState(chatControl, "OfflineState", true);

            if (byUser)
                autoSignout.OnSignOut();
        }

        void OnSignIn(SignInOptions signInOptions)
        {
            CreateMonitor();

            VisualStateManager.GoToState(chatControl, "OnlineState", true);
            autoSignout.OnSignIn(signInOptions);

            foreach (var window in chatWindows)
                window.Enabled = true;

            CheckForUpdates();
        }

        void SignOut(bool byUser)
        {
            if (context.ChatClient == null || !context.ChatClient.IsLoggedIn)
                return;

            clientAvailable.Begin();

            Dispatcher.Invoke((Action)(() =>
            {
                // Can't chat with chat service stopped. End all sessions.
                foreach (var window in chatWindows)
                    window.EndChat();

                DestroyMonitor();

                chatControl.SignIn.DisplayName = context.ChatClient.CurrentUser.DisplayName;
                chatControl.SignIn.GroupName = context.ChatClient.CurrentUser.Properties.GroupName;

                Async.Invoke(() =>
                {
                    ExceptionMonster.EatTheException(() => context.ChatClient.Logout(), "logging out client");
                    clientAvailable.End();
                });

                OnSignout(byUser);
            }));
        }

        void UpdateSortMenu()
        {
            mnuSortByStatus.IsChecked = (SettingsProvider.Current.Settings.ContactSettings.ContactListSortField == ContactListSortField.Status);
            mnuSortByName.IsChecked = !mnuSortByStatus.IsChecked;
        }

        void UpdateGroupMenu()
        {
            mnuGroupBuddies.IsChecked = SettingsProvider.Current.Settings.ContactSettings.GroupContacts;
        }

        void CheckForUpdates()
        {
            if (!SettingsProvider.Current.Settings.GeneralSettings.CheckForUpdates)
                return;

            UpdateCheckResult result = null;
            Async.Invoke(() => result = UpdateNotifier.CheckForUpdate(SettingsProvider.Current.Settings.GeneralSettings.FirstRun),
                         () => OnUpdateCheckComplete(result));
        }

        static void AddGroupName(string groupName)
        {
            if (!groupName.Equals(BuddyProperties.DefaultGroupName))
            {
                SettingsProvider.Current.Settings.ContactSettings.ContactGroups.Add(groupName);
                SettingsProvider.Current.Save();
            }
        }

        void CreateMonitor()
        {
            TimeSpan timeout = SettingsProvider.Current.Settings.PersonalSettings.IdleTimeout.Minutes();
            idleStatusChanger = new IdleStatusChanger(context.ChatClient, timeout);
        }

        void DestroyMonitor()
        {
            idleStatusChanger.Dispose();
            idleStatusChanger = null;
        }        

        IChatWindow StartChat(IBuddy buddy, bool sendFile, params string[] filePaths)
        {
            IChatWindow window = StartChat(buddy);
            if (sendFile)
                if (filePaths == null || filePaths.Length == 0)
                    window.SendFile();
                else
                    window.SendFiles(filePaths);

            return window;
        }

        ChatWindow CreateChatWindow(IBuddy buddy, IChat chatSession, bool initiatedByUser)
        {
            ChatWindow window = null;

            if (chatSession == null || !chatSession.IsGroupChat)
                window = chatWindows.Find(w => buddy.Equals(w.PrimaryBuddy) && !w.IsGroupChat);

            if (window == null)
            {
                window = new ChatWindow(buddy, context);
                window.Closed += (sender, e) => chatWindows.Remove(window);
                window.SetChatSession(chatSession ?? context.ChatClient.StartChat(buddy));
                chatWindows.Add(window);
            }
            else if (chatSession != null)
                window.SetChatSession(chatSession);

            window.Show(initiatedByUser);

            return window;
        }

        void SetupControls()
        {
            StateChanged += Window_StateChanged;

            chatControl.SignIn.Configure(context.PluginLoader.AuthenticationProvider);
            chatControl.SignIn.LoginInitiated += SignInControl_LoginInitiated;
            chatControl.ContactList.BroadcastChatStart += ContactList_BroadcastChatStart;
            chatControl.ContactList.GroupChatStart += ContactList_GroupChatStart;
            chatControl.ContactList.ChatStart += ContactList_StartChat;
            chatControl.ContactList.SignOut += ContactList_SignOut;
            chatControl.ContactList.OpenAbout += (sender, e) => SquiggleUtility.ShowAboutDialog(this);

            UpdateSortMenu();
            UpdateGroupMenu();
        }

        void LoadPosition()
        {
            this.Height = Properties.Settings.Default.MainWindowHeight;
            this.Width = Properties.Settings.Default.MainWindowWidth;

            this.Top = Properties.Settings.Default.MainWindowTop > 0 ? Properties.Settings.Default.MainWindowTop : System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2 - this.Height / 2;
            this.Left = Properties.Settings.Default.MainWindowLeft > 0 ? Properties.Settings.Default.MainWindowLeft : System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2 - this.Width / 2;

            if (App.RunInBackground)
                this.Hide();
        } 
    }
}
