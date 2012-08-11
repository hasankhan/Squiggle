using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Squiggle.Activities;
using Squiggle.Chat;
using Squiggle.Plugins;
using Squiggle.Plugins.MessageFilter;
using Squiggle.UI.Components;
using Squiggle.UI.Controls;
using Squiggle.UI.Controls.ChatItems;
using Squiggle.UI.Helpers;
using Squiggle.UI.Helpers.Collections;
using Squiggle.UI.MessageFilters;
using Squiggle.UI.MessageParsers;
using Squiggle.UI.Resources;
using Squiggle.UI.Settings;
using Squiggle.UI.StickyWindow;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.Chat.Activities;

namespace Squiggle.UI.Windows
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : StickyWindowBase, IChatWindow
    {
        IChat chatSession;
        FlashWindow flash;
        DateTime? lastMessageReceived;
        DispatcherTimer statusResetTimer;
        ActionQueue eventQueue = new ActionQueue();
        DateTime? lastBuzzSent;
        DateTime? lastBuzzReceived;
        string lastSavedFile;
        string lastSavedFormat;
        bool buzzPending;
        WindowState lastState;
        FileTransferCollection fileTransfers = new FileTransferCollection();
        static Dictionary<Buddy, IEnumerable<ChatItem>> chatHistory = new Dictionary<Buddy, IEnumerable<ChatItem>>();
        SquiggleContext context;
        MultiFilter filters = new MultiFilter();
        MultiParser parsers = new MultiParser();
        bool chatStarted;

        public Buddy PrimaryBuddy { get; private set; }

        public ChatWindow()
        {
            InitializeComponent();
        }

        internal ChatWindow(Buddy buddy, SquiggleContext context) : this()
        {
            this.context = context;

            filters.AddRange(context.PluginLoader.MessageFilters);
            parsers.AddRange(context.PluginLoader.MessageParsers);

            this.Height = Properties.Settings.Default.ChatWindowHeight;
            this.Width = Properties.Settings.Default.ChatWindowWidth;

            expanderDisplayPics.IsExpanded = Properties.Settings.Default.ChatWindowShowDisplayPictures;

            statusResetTimer = new DispatcherTimer();
            statusResetTimer.Interval = 5.Seconds();
            statusResetTimer.Tick += (sender, e) => ResetStatus();
            this.StateChanged += new EventHandler(ChatWindow_StateChanged);

            SettingsProvider.Current.SettingsUpdated += (sender, e) => LoadSettings();
            LoadSettings();
            this.DataContext = this;

            this.PrimaryBuddy = buddy;
            this.PrimaryBuddy.Online += new EventHandler(buddy_Online);
            this.PrimaryBuddy.Offline += new EventHandler(buddy_Offline);

            chatTextBox.KeepHistory = !SettingsProvider.Current.Settings.ChatSettings.ClearChatOnWindowClose;

            eventQueue.Enqueue(() =>
            {
                if (!IsGroupChat && chatTextBox.KeepHistory)
                {
                    IEnumerable<ChatItem> history;
                    if (chatHistory.TryGetValue(buddy, out history))
                        chatTextBox.AddItems(history);
                }
            });

            new ActivitiesMenuHelper(context).LoadActivities(mnuStartActivity, mnuNoActivity, new RelayCommand<IActivity>(StartActivityMenuItem_Click));
        }

        public IEnumerable<Buddy> Buddies
        {
            get
            {
                if (chatSession == null)
                    return Enumerable.Empty<Buddy>();
                else
                    return chatSession.Buddies;
            }
        }

        public bool IsGroupChat
        {
            get
            {
                if (chatSession == null)
                    return false;
                else
                    return chatSession.IsGroupChat;
            }
        }

        public bool Enabled
        {
            get { return txtMessageEditBox.Enabled; }
            set { txtMessageEditBox.Enabled = value; }
        }

        bool IsBroadcastChat
        {
            get { return (chatSession is BroadcastChat); }
        }

        void LoadSettings()
        {
            if (SettingsProvider.Current.Settings.ChatSettings.ClearChatOnWindowClose)
                chatHistory.Clear();

            parsers.Remove(parsers.OfType<EmoticonParser>().First());
            if (SettingsProvider.Current.Settings.ChatSettings.ShowEmoticons)
                parsers.Add(context.PluginLoader.MessageParsers.OfType<EmoticonParser>().First());

            txtMessageEditBox.txtMessage.SpellCheck.IsEnabled = SettingsProvider.Current.Settings.ChatSettings.SpellCheck;

            if (chatSession != null)
                chatSession.EnableLogging = SettingsProvider.Current.Settings.ChatSettings.EnableLogging;
        }

        internal void SetChatSession(IChat chat)
        {
            if (chat == null)
                return;

            DestroySession();
            chatSession = chat;
            chatSession.BuzzReceived += new EventHandler<BuddyEventArgs>(chatSession_BuzzReceived);
            chatSession.MessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(chatSession_MessageReceived);
            chatSession.BuddyJoined += new EventHandler<BuddyEventArgs>(chatSession_BuddyJoined);
            chatSession.BuddyLeft += new EventHandler<BuddyEventArgs>(chatSession_BuddyLeft);
            chatSession.MessageFailed += new EventHandler<MessageFailedEventArgs>(chatSession_MessageFailed);
            chatSession.BuddyTyping += new EventHandler<BuddyEventArgs>(chatSession_BuddyTyping);
            chatSession.ActivityInvitationReceived += new EventHandler<ActivityInvitationReceivedEventArgs>(chatSession_ActivityInvitationReceived);
            chatSession.GroupChatStarted += new EventHandler(chatSession_GroupChatStarted);
            txtMessageEditBox.Enabled = true;
            mnuInviteContact.IsEnabled = !IsBroadcastChat;
            chatSession.EnableLogging = SettingsProvider.Current.Settings.ChatSettings.EnableLogging;
            MonitorAll();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            messagePanel.Height = new GridLength(SettingsProvider.Current.Settings.GeneralSettings.MessagePanelHeight, GridUnitType.Pixel);
            flash = new FlashWindow(this);

            OnParticipantsChanged();
            eventQueue.Open();
        }

        void ChatWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Minimized)
                lastState = this.WindowState;

            Dispatcher.Invoke(() =>
            {
                if (this.WindowState != System.Windows.WindowState.Minimized)
                {
                    if (buzzPending)
                    {
                        Dispatcher.Delay(() =>
                        {
                            DoBuzzAction();
                        }, TimeSpan.FromSeconds(.5));
                        buzzPending = false;
                    }
                }
            });
        }

        private void txtMessage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    e.Handled = true;
        }

        private void txtMessageEditBox_MessageSend(object sender, MessageSendEventArgs e)
        {
            SendMessage(e.Message);
        }

        private void txtMessageEditBox_MessageTyping(object sender, EventArgs e)
        {
            if (chatSession != null)
                chatSession.NotifyTyping();
        }

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            SendFile();
            txtMessageEditBox.GetFocus();
        }

        private void ChangeFont_Click(object sender, RoutedEventArgs e)
        {
            SquiggleUtility.ShowFontDialog();
            txtMessageEditBox.GetFocus();
        }

        private void SendBuzz_Click(object sender, RoutedEventArgs e)
        {
            SendBuzz();
            txtMessageEditBox.GetFocus();
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
            SendFile();
        }

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        void chatSession_GroupChatStarted(object sender, EventArgs e)
        {
            eventQueue.Enqueue(OnGroupChatStarted);
        }

        void chatSession_ActivityInvitationReceived(object sender, ActivityInvitationReceivedEventArgs e)
        {
            eventQueue.Enqueue(() => OnActivityInvite(e));
        }        

        void chatSession_BuzzReceived(object sender, BuddyEventArgs e)
        {
            eventQueue.Enqueue(() => OnBuzzReceived(e.Buddy));
        }

        void chatSession_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            eventQueue.Enqueue(() => OnMessageReceived(e.Sender, e.Message, e.FontName, e.Color, e.FontSize, e.FontStyle));
        }

        void chatSession_BuddyTyping(object sender, BuddyEventArgs e)
        {
            eventQueue.Enqueue(() => OnBuddyTyping(e));
        }

        void chatSession_MessageFailed(object sender, MessageFailedEventArgs e)
        {
            eventQueue.Enqueue(() => OnMessageFailed(e));
        }

        void chatSession_BuddyLeft(object sender, BuddyEventArgs e)
        {
            eventQueue.Enqueue(() => OnBuddyLeft(e.Buddy));
        }

        void chatSession_BuddyJoined(object sender, BuddyEventArgs e)
        {
            eventQueue.Enqueue(() => OnBuddyJoined(e.Buddy));
        }

        void buddy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(OnParticipantsChanged);
        }

        void buddy_Online(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!IsGroupChat && Buddies.Contains(PrimaryBuddy))
                    buddyOfflineMessage.Visibility = Visibility.Collapsed;
            });
        }

        void buddy_Offline(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!IsGroupChat && Buddies.Contains(PrimaryBuddy))
                {
                    buddyOfflineMessage.DataContext = PrimaryBuddy.DisplayName;
                    buddyOfflineMessage.Visibility = Visibility.Visible;
                }
            });
        }

        void OnActivityInvite(ActivityInvitationReceivedEventArgs e)
        {
            IActivityHandler handler = context.PluginLoader.GetActivityHandler(e.ActivityId, f => f.FromInvite(e.Executor, e.Metadata));
            if (e.ActivityId == SquiggleActivities.VoiceChat)
                OnVoiceInvite(handler as IVoiceChatHandler);
            else if (e.ActivityId == SquiggleActivities.FileTransfer)
                OnTransferInvite(handler as IFileTransferHandler);
            else
                OnUnknownActivityInvite(handler);
        }        

        void OnBuddyTyping(BuddyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ChangeStatus(String.Format("{0} " + Translation.Instance.ChatWindow_IsTyping, e.Buddy.DisplayName));
                statusResetTimer.Stop();
                statusResetTimer.Start();
            });
        }

        void OnMessageFailed(MessageFailedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
#if DEBUG
                string message = "Following message could not be delivered due to error: " + e.Exception.Message;
#else
                string message = Translation.Instance.ChatWindow_MessageCouldNotBeDelivered;
#endif
                string detail = e.Message;
                chatTextBox.AddError(message, detail);
            });
        }

        void OnBuddyJoined(Buddy buddy)
        {
            Monitor(buddy);
            Dispatcher.Invoke(() =>
            {
                chatTextBox.AddInfo(String.Format("{0} " + Translation.Instance.ChatWindow_HasJoinedConversation, buddy.DisplayName));
                OnParticipantsChanged();
            });
        }

        void OnBuddyLeft(Buddy buddy)
        {
            StopMonitoring(buddy);
            Dispatcher.Invoke(() =>
            {
                chatTextBox.AddInfo(String.Format("{0} " + Translation.Instance.ChatWindow_HasLeftConversation, buddy.DisplayName));
                OnParticipantsChanged();
            });
        }

        void OnParticipantsChanged()
        {
            UpdateGroupChatControls();
            UpdateTitle();
            UpdateDisplayPicPanel();
        }

        void UpdateDisplayPicPanel()
        {
            var itemsView = CollectionViewSource.GetDefaultView(displayPicsItemControl.ItemsSource);
            itemsView.Refresh();
        }

        void Monitor(Buddy buddy)
        {
            StopMonitoring(buddy);
            buddy.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(buddy_PropertyChanged);
        }

        void MonitorAll()
        {
            foreach (Buddy buddy in Buddies)
                Monitor(buddy);
        }

        void StopMonitoring(Buddy buddy)
        {
            buddy.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(buddy_PropertyChanged);
        }

        void StopMonitoringAll()
        {
            foreach (Buddy buddy in Buddies)
                StopMonitoring(buddy);
        }

        void OnBuzzReceived(Buddy buddy)
        {
            Dispatcher.Invoke(() =>
            {
                if (lastBuzzReceived == null || DateTime.Now.Subtract(lastBuzzReceived.Value).TotalSeconds > 5)
                {
                    chatTextBox.AddInfo(String.Format("{0} " + Translation.Instance.ChatWindow_HasSentYouBuzz, buddy.DisplayName));
                    if (this.WindowState != System.Windows.WindowState.Minimized)
                        DoBuzzAction();
                    else
                        buzzPending = true;
                    FlashWindow();
                    lastBuzzReceived = DateTime.Now;
                }
            });
        }

        void DoBuzzAction()
        {
            AudioAlert.Instance.Play(AudioAlertType.Buzz);
            SquiggleUtility.ShakeWindow(this);
        }

        void PlayAlert(AudioAlertType alertType)
        {
            if (!IsActive)
                AudioAlert.Instance.Play(alertType);
        }

        void OnMessageReceived(Buddy buddy, string message, string fontName, System.Drawing.Color color, int fontSize, System.Drawing.FontStyle fontStyle)
        {
            Dispatcher.Invoke(() =>
            {
                var temp = new StringBuilder(message);
                if (filters.Filter(temp, this, FilterDirection.In))
                {
                    message = temp.ToString();
                    lastMessageReceived = DateTime.Now;
                    chatTextBox.AddMessage(buddy.DisplayName, message, fontName, fontSize, fontStyle, color, parsers);
                    ResetStatus();
                    FlashWindow();
                    if (!chatStarted && !IsActive)
                        TrayPopup.Instance.Show(Translation.Instance.Popup_NewMessage, String.Format("{0} " + Translation.Instance.Global_ContactSays + ": {1}", buddy.DisplayName, message), args => this.Restore());
                    PlayAlert(AudioAlertType.MessageReceived);
                }
            });
            chatStarted = true;
        }

        void OnGroupChatStarted()
        {
            Dispatcher.Invoke(() =>
            {
                MonitorAll();
                OnParticipantsChanged();
            });
        }

        void OnTransferInvite(IFileTransferHandler invitation)
        {
            Dispatcher.Invoke(() =>
            {
                FlashWindow();
                if (invitation == null)
                {
                    chatTextBox.AddInfo(Translation.Instance.ChatWindow_FileTransferInviteNotSupported);
                    return;
                }
                string downloadsFolder = SettingsProvider.Current.Settings.GeneralSettings.DownloadsFolder;
                downloadsFolder = Path.Combine(downloadsFolder, PrimaryBuddy.DisplayName);
                chatTextBox.AddFileReceiveRequest(invitation, downloadsFolder);
                fileTransfers.Add(invitation);
            });
            chatStarted = true;
        }

        void OnUnknownActivityInvite(IActivityHandler handler)
        {
            Dispatcher.Invoke(() =>
            {
                FlashWindow();
                if (handler == null)
                    chatTextBox.AddInfo(Translation.Instance.ChatWindow_UnknownActivityInvite);
            });
        }

        void OnVoiceInvite(IVoiceChatHandler invitation)
        {
            Dispatcher.Invoke(() =>
            {
                FlashWindow();
                if (invitation == null)
                {
                    chatTextBox.AddInfo(Translation.Instance.ChatWindow_VoiceChatInviteNotSupported);
                    return;
                }
                chatTextBox.AddVoiceChatReceivedRequest(context, invitation, PrimaryBuddy.DisplayName, context.IsVoiceChatActive);
                voiceController.VoiceChatContext = invitation;
                invitation.Dispatcher = Dispatcher;
            });
            chatStarted = true;
        }

        public void SendMessage(string message)
        {
            chatStarted = true;
            if (chatSession == null)
            {
                var buddyInList = context.ChatClient.Buddies.FirstOrDefault(b => b.Equals(PrimaryBuddy));
                if (buddyInList == null)
                {
                    OnMessageFailed(new MessageFailedEventArgs() { Message = message, Exception = null });
                    return;
                }
                else
                {
                    PrimaryBuddy = buddyInList;
                    SetChatSession(buddyInList.StartChat());
                }
            }

            string displayName = context.ChatClient == null ? Translation.Instance.Global_You : context.ChatClient.CurrentUser.DisplayName;
            var settings = SettingsProvider.Current.Settings.PersonalSettings;

            var temp = new StringBuilder(message);
            if (filters.Filter(temp, this, FilterDirection.Out))
            {
                message = temp.ToString();
                chatSession.SendMessage(settings.FontName, settings.FontSize, settings.FontColor, settings.FontStyle, message);
                chatTextBox.AddMessage(displayName, message, settings.FontName, settings.FontSize, settings.FontStyle, settings.FontColor, parsers);
            }
        }

        public void SendBuzz()
        {
            if (chatSession == null)
                return;

            if (lastBuzzSent == null || DateTime.Now.Subtract(lastBuzzSent.Value).TotalSeconds > 5)
            {
                chatTextBox.AddInfo(Translation.Instance.ChatWindow_YouSentBuzz);
                chatSession.SendBuzz();
                lastBuzzSent = DateTime.Now;
                DoBuzzAction();
            }
            else
                chatTextBox.AddError(Translation.Instance.ChatWindow_BuzzTooEarly, String.Empty);
        }

        public IVoiceChatHandler StartVoiceChat()
        {
            if (chatSession.IsGroupChat)
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_VoiceChatNotAllowedInGroup, String.Empty);
                return null;
            }
            else if (context.IsVoiceChatActive)
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_AlreadyInVoiceChat, String.Empty);
                return null;
            }
            else if (!context.PluginLoader.HasActivity(SquiggleActivities.VoiceChat))
                return null;

            IActivityExecutor executor = chatSession.CreateActivity();
            IVoiceChatHandler voiceChat = context.PluginLoader.GetActivityHandler(SquiggleActivities.VoiceChat, f => f.CreateInvite(executor, null)) as IVoiceChatHandler;

            if (voiceChat != null)
            {
                voiceChat.Dispatcher = Dispatcher;
                voiceChat.Start();
                chatTextBox.AddVoiceChatSentRequest(context, voiceChat, PrimaryBuddy.DisplayName);
                chatStarted = true;
            }
            
            return voiceChat;
        }

        public void SendFile()
        {
            if (chatSession.IsGroupChat)
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_FileTransferNotAllowedInGroup, String.Empty);
                return;
            }

            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SendFiles(dialog.FileName);
            }
        }

        public void SendFiles(params string[] filePaths)
        {
            foreach (var filePath in filePaths)
                SendFile(filePath);
        }

        public void SendFile(string filePath)
        {
            if (chatSession.IsGroupChat)
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_FileTransferNotAllowedInGroup, String.Empty);
                return;
            }

            if (!context.PluginLoader.HasActivity(SquiggleActivities.FileTransfer))
                return;

            if (File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                FileStream fileStream = null;

                if (!ExceptionMonster.EatTheException(() =>
                                                        {
                                                            fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
                                                        }, "opening the file for transfer"))
                {
                    chatTextBox.AddError(String.Format(Translation.Instance.ChatWindow_CouldNotReadFile + "'{0}'" + Translation.Instance.ChatWindow_MakeSureFileNotInUse, fileName), null);
                    return;
                }

                IActivityExecutor executor = chatSession.CreateActivity();
                var args = new Dictionary<string, object>(){ { "name", fileName}, {"content", fileStream}, {"size", fileStream.Length}};
                IFileTransferHandler fileTransfer = context.PluginLoader.GetActivityHandler(SquiggleActivities.FileTransfer, f => f.CreateInvite(executor, args)) as IFileTransferHandler;
                if (fileTransfer == null)
                    return;

                fileTransfer.Start();
                fileTransfers.Add(fileTransfer);
                chatTextBox.AddFileSentRequest(fileTransfer);

                chatStarted = true;
            }
        }

        public void SaveAs()
        {
            string file, format;
            if (ShowSaveDialog(out file, out format))
                SaveTo(file, format);
        }

        public void Save()
        {
            if (String.IsNullOrEmpty(lastSavedFile))
            {
                if (ShowSaveDialog(out lastSavedFile, out lastSavedFormat))
                    Save();
            }
            else
                SaveTo(lastSavedFile, lastSavedFormat);
        }

        public void SaveTo(string fileName, string format)
        {
            chatTextBox.SaveTo(fileName);

            if (format != DataFormats.Rtf)
            {
                var richTextBox = new System.Windows.Forms.RichTextBox();
                richTextBox.LoadFile(fileName);
                if (format == DataFormats.UnicodeText)
                    richTextBox.SaveFile(fileName, System.Windows.Forms.RichTextBoxStreamType.UnicodePlainText);
                else
                    richTextBox.SaveFile(fileName, System.Windows.Forms.RichTextBoxStreamType.PlainText);
            }
        }

        public void Invite(IEnumerable<Buddy> buddies)
        {
            foreach (Buddy buddy in buddies)
                Invite(buddy);
        }

        public void Invite(Buddy buddy)
        {
            if (buddy != null && chatSession != null && !Buddies.Contains(buddy))
                chatSession.Invite(buddy);
        }

        public void Restore()
        {
            if (Visibility == System.Windows.Visibility.Collapsed)
                Visibility = System.Windows.Visibility.Visible;

            if (WindowState == System.Windows.WindowState.Minimized)
                WindowState = lastState;

            this.Activate();
        }

        internal void DestroySession()
        {
            if (voiceController.VoiceChatContext != null)
                voiceController.VoiceChatContext.Cancel();

            fileTransfers.CancelAll();
            if (chatSession != null)
            {
                StopMonitoringAll();

                chatSession.BuzzReceived -= new EventHandler<BuddyEventArgs>(chatSession_BuzzReceived);
                chatSession.MessageReceived -= new EventHandler<ChatMessageReceivedEventArgs>(chatSession_MessageReceived);
                chatSession.BuddyJoined -= new EventHandler<BuddyEventArgs>(chatSession_BuddyJoined);
                chatSession.BuddyLeft -= new EventHandler<BuddyEventArgs>(chatSession_BuddyLeft);
                chatSession.MessageFailed -= new EventHandler<MessageFailedEventArgs>(chatSession_MessageFailed);
                chatSession.BuddyTyping -= new EventHandler<BuddyEventArgs>(chatSession_BuddyTyping);
                chatSession.ActivityInvitationReceived -= new EventHandler<ActivityInvitationReceivedEventArgs>(chatSession_ActivityInvitationReceived);
                chatSession.GroupChatStarted -= new EventHandler(chatSession_GroupChatStarted);
                chatSession.Leave();
                chatSession = null;
                Dispatcher.Invoke(() =>
                {
                    OnParticipantsChanged();
                    txtMessageEditBox.Enabled = false;
                });
            }
        }

        void UpdateTitle()
        {
            if (IsBroadcastChat)
                this.Title = Translation.Instance.ChatWindow_BroadCastChatTitle;
            else
            {
                string title = String.Join(", ", Buddies.Select(b => b.DisplayName).ToArray());
                this.Title = String.IsNullOrEmpty(title) ? PrimaryBuddy.DisplayName : title;
            }
        }

        void ChangeStatus(string message, params object[] args)
        {
            txbStatus.Text = String.Format(message, args);
        }

        void ResetStatus()
        {
            statusResetTimer.Stop();
            if (!lastMessageReceived.HasValue)
                ChangeStatus(String.Empty);
            else
                ChangeStatus(Translation.Instance.ChatWindow_LastMessageAt + String.Format(" {0:T} on {0:d}", lastMessageReceived));
        }

        void FlashWindow()
        {
            if (this.Visibility == System.Windows.Visibility.Collapsed)
                Show(false);

            flash.Start();
        }

        internal void Show(bool initiatedByUser)
        {
            if (!initiatedByUser)
            {
                if (!SettingsProvider.Current.Settings.ChatSettings.StealFocusOnNewMessage)
                    WindowState = WindowState.Minimized;
            }

            Show();

            if (initiatedByUser || SettingsProvider.Current.Settings.ChatSettings.StealFocusOnNewMessage)
                Restore();
        }

        static bool ShowSaveDialog(out string fileName, out string format)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "RTF Document|*.rtf|Unicode Text Document|*.txt|Text Document|*.txt";
            dialog.CheckPathExists = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = dialog.FileName;
                if (dialog.FilterIndex == 1)
                    format = DataFormats.Rtf;
                else if (dialog.FilterIndex == 2)
                    format = DataFormats.UnicodeText;
                else
                    format = DataFormats.Text;
                return true;
            }
            fileName = null;
            format = null;
            return false;
        }

        private void UndoMenu_Click(object sender, RoutedEventArgs e)
        {
            txtMessageEditBox.txtMessage.Undo();
        }

        private void CutMenu_Click(object sender, RoutedEventArgs e)
        {
            txtMessageEditBox.txtMessage.Cut();
        }

        private void CopyMenu_Click(object sender, RoutedEventArgs e)
        {
            txtMessageEditBox.txtMessage.Copy();
        }

        private void PasteMenu_Click(object sender, RoutedEventArgs e)
        {
            txtMessageEditBox.txtMessage.Paste();
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            txtMessageEditBox.txtMessage.SelectedText = String.Empty;
        }

        private void SelectAllMenu_Click(object sender, RoutedEventArgs e)
        {
            txtMessageEditBox.txtMessage.SelectAll();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Normal)
            {
                Properties.Settings.Default.ChatWindowHeight = this.Height;
                Properties.Settings.Default.ChatWindowWidth = this.Width;
                Properties.Settings.Default.Save();
            }
        }

        private void InviteContactMenu_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Buddy> buddies = SquiggleUtility.SelectContacts(Translation.Instance.ChatWindow_InviteContact, this, b => Buddies.Contains(b));
            Invite(buddies);
        }

        private void StartActivityMenuItem_Click(IActivity activity)
        {
            MessageBox.Show("Launching activity " + activity.Title);
        }

        private void SendEmoticon_Click(object sender, RoutedEventArgs e)
        {
            Point pos = PointToScreen(Mouse.GetPosition(this));
            var selector = new EmoticonSelector();
            selector.EmoticonSelected += (s1, e1) => OnEmoticonSelected(((EmoticonSelector)s1).Code);
            selector.Top = pos.Y;
            selector.Left = pos.X;
            selector.Show();
        }

        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            string to = String.Join(";", Buddies.Select(buddy => buddy.Properties.EmailAddress).ToArray());
            System.Diagnostics.Process.Start("mailto:" + to);
        }

        void OnEmoticonSelected(string code)
        {
            txtMessageEditBox.txtMessage.SelectedText = code;
            txtMessageEditBox.txtMessage.SelectionStart += code.Length;
            txtMessageEditBox.txtMessage.SelectionLength = 0;
        }

        void UpdateGroupChatControls()
        {
            bool singleSession = (chatSession == null || !chatSession.IsGroupChat);
            btnSendFile.IsEnabled = mnuSendFile.IsEnabled = context.PluginLoader.HasActivity(SquiggleActivities.FileTransfer) && singleSession;
            voiceController.IsEnabled = context.PluginLoader.HasActivity(SquiggleActivities.VoiceChat) && singleSession;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            txtMessageEditBox.GetFocus();
        }

        bool forceClose;
        internal void ForceClose()
        {
            forceClose = true;
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!forceClose && fileTransfers.Any())
            {
                e.Cancel = !SquiggleUtility.Confirm(ConfirmationDialogType.FileTransferWindowClose, this);
                if (e.Cancel)
                    return;
            }

            DestroySession();

            var history = chatTextBox.GetHistory();

            if (!(IsGroupChat || SettingsProvider.Current.Settings.ChatSettings.ClearChatOnWindowClose))
                chatHistory[PrimaryBuddy] = history;

            history.RemoveAll();

            base.OnClosing(e);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SettingsProvider.Current.Settings.GeneralSettings.MessagePanelHeight = messagePanel.Height.Value;
            SettingsProvider.Current.Save();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            ChangeExpanderState(true);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            ChangeExpanderState(false);
        }

        private static void ChangeExpanderState(bool show)
        {
            Properties.Settings.Default.ChatWindowShowDisplayPictures = show;
            Properties.Settings.Default.Save();
        }

        private void VoiceChatToolbarControl_StartChat(object sender, EventArgs e)
        {
            IVoiceChatHandler voiceChat = StartVoiceChat();
            ((VoiceChatToolbarControl)sender).VoiceChatContext = voiceChat;
        }
    }
}
