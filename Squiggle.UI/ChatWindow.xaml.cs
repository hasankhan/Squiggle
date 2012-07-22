using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Squiggle.Chat;
using Squiggle.Core.Chat.FileTransfer;
using Squiggle.Core.Chat.Voice;
using Squiggle.UI.Controls;
using Squiggle.UI.Controls.ChatItems;
using Squiggle.UI.Helpers;
using Squiggle.UI.MessageFilters;
using Squiggle.UI.MessageParsers;
using Squiggle.UI.Resources;
using Squiggle.UI.Settings;
using Squiggle.UI.StickyWindows;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : StickyWindow
    {
        IChat chatSession;
        FlashWindow flash;
        DateTime? lastMessageReceived;
        DispatcherTimer statusResetTimer;
        ActionQueue eventQueue = new ActionQueue();
        DateTime? lastBuzzSent;
        DateTime? lastBuzzReceived;
        bool loaded;
        string lastSavedFile;
        string lastSavedFormat;
        bool buzzPending;
        WindowState lastState;
        FileTransferCollection fileTransfers = new FileTransferCollection();
        static Dictionary<Buddy, IEnumerable<ChatItem>> chatHistory = new Dictionary<Buddy, IEnumerable<ChatItem>>(); 

        MultiFilter filters = new MultiFilter();
        bool chatStarted;

        public Buddy PrimaryBuddy { get; private set; }

        public ChatWindow()
        {
            InitializeComponent();

            this.Height = Properties.Settings.Default.ChatWindowHeight;
            this.Width = Properties.Settings.Default.ChatWindowWidth;

            expanderDisplayPics.IsExpanded = Properties.Settings.Default.ChatWindowShowDisplayPictures;

            statusResetTimer = new DispatcherTimer();
            statusResetTimer.Interval = TimeSpan.FromSeconds(5);
            statusResetTimer.Tick += (sender, e) => ResetStatus();
            this.StateChanged += new EventHandler(ChatWindow_StateChanged);

            SettingsProvider.Current.SettingsUpdated += (sender, e) => LoadSettings();
            LoadSettings();

            filters.Add(AliasFilter.Instance);
            filters.Add(CommandsFilter.Instance);

            this.DataContext = this;
        }

        public ChatWindow(Buddy buddy)
            : this()
        {
            this.PrimaryBuddy = buddy;
            this.PrimaryBuddy.Online += new EventHandler(buddy_Online);
            this.PrimaryBuddy.Offline += new EventHandler(buddy_Offline);

            chatTextBox.KeepHistory = !SettingsProvider.Current.Settings.ChatSettings.ClearChatOnWindowClose;

            DeferIfNotLoaded(() =>
            {
                if (!IsGroupChat && chatTextBox.KeepHistory)
                {
                    IEnumerable<ChatItem> history;
                    if (chatHistory.TryGetValue(buddy, out history))
                        chatTextBox.AddItems(history);
                }
            });
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

            chatTextBox.MessageParsers.Remove(EmoticonParser.Instance);
            if (SettingsProvider.Current.Settings.ChatSettings.ShowEmoticons)
                chatTextBox.MessageParsers.Add(EmoticonParser.Instance);

            txtMessageEditBox.txtMessage.SpellCheck.IsEnabled = SettingsProvider.Current.Settings.ChatSettings.SpellCheck;

            if (chatSession != null)
                chatSession.EnableLogging = SettingsProvider.Current.Settings.ChatSettings.EnableLogging;
        }

        public void SetChatSession(IChat chat)
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
            chatSession.TransferInvitationReceived += new EventHandler<FileTransferInviteEventArgs>(chatSession_TransferInvitationReceived);
            chatSession.VoiceChatInvitationReceived += new EventHandler<VoiceChatInviteEventArgs>(chatSession_VoiceChatInvitationReceived);
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
            lock (eventQueue)
            {
                loaded = true;
                eventQueue.DequeueAll();
            }
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
            DeferIfNotLoaded(OnGroupChatStarted);
        }

        void chatSession_TransferInvitationReceived(object sender, FileTransferInviteEventArgs e)
        {
            DeferIfNotLoaded(() => OnTransferInvite(e));
        }


        void chatSession_VoiceChatInvitationReceived(object sender, VoiceChatInviteEventArgs e)
        {
            DeferIfNotLoaded(() => OnVoiceInvite(e));
        }

        void chatSession_BuzzReceived(object sender, BuddyEventArgs e)
        {
            DeferIfNotLoaded(() => OnBuzzReceived(e.Buddy));
        }

        void chatSession_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            DeferIfNotLoaded(() => OnMessageReceived(e.Sender, e.Message, e.FontName, e.Color, e.FontSize, e.FontStyle));
        }

        void chatSession_BuddyTyping(object sender, BuddyEventArgs e)
        {
            DeferIfNotLoaded(() => OnBuddyTyping(e));
        }

        void chatSession_MessageFailed(object sender, MessageFailedEventArgs e)
        {
            DeferIfNotLoaded(() => OnMessageFailed(e));
        }

        void chatSession_BuddyLeft(object sender, BuddyEventArgs e)
        {
            DeferIfNotLoaded(() => OnBuddyLeft(e.Buddy));
        }

        void chatSession_BuddyJoined(object sender, BuddyEventArgs e)
        {
            DeferIfNotLoaded(() => OnBuddyJoined(e.Buddy));
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
                    chatTextBox.AddMessage(buddy.DisplayName, message, fontName, fontSize, fontStyle, color);
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

        void OnTransferInvite(FileTransferInviteEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                string downloadsFolder = SettingsProvider.Current.Settings.GeneralSettings.DownloadsFolder;
                chatTextBox.AddFileReceiveRequest(e.Invitation, downloadsFolder);
                fileTransfers.Add(e.Invitation);
                FlashWindow();
            });
            chatStarted = true;
        }

        void OnVoiceInvite(VoiceChatInviteEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                chatTextBox.AddVoiceChatReceivedRequest(e.Invitation, e.Sender.DisplayName, MainWindow.Instance.ChatClient.IsVoiceChatActive());
                voiceController.VoiceChatContext = e.Invitation;
                e.Invitation.Dispatcher = Dispatcher;
                FlashWindow();
            });
            chatStarted = true;
        }

        public void SendMessage(string message)
        {
            chatStarted = true;
            if (chatSession == null)
            {
                var buddyInList = MainWindow.Instance.ChatClient.Buddies.FirstOrDefault(b => b.Equals(PrimaryBuddy));
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

            string displayName = MainWindow.Instance.ChatClient == null ? Translation.Instance.Global_You : MainWindow.Instance.ChatClient.CurrentUser.DisplayName;
            var settings = SettingsProvider.Current.Settings.PersonalSettings;

            var temp = new StringBuilder(message);
            if (filters.Filter(temp, this, FilterDirection.Out))
            {
                message = temp.ToString();
                chatSession.SendMessage(settings.FontName, settings.FontSize, settings.FontColor, settings.FontStyle, message);
                chatTextBox.AddMessage(displayName, message, settings.FontName, settings.FontSize, settings.FontStyle, settings.FontColor);
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

        public IVoiceChat StartVoiceChat()
        {
            if (chatSession.IsGroupChat)
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_VoiceChatNotAllowedInGroup, String.Empty);
                return null;
            }
            else if (MainWindow.Instance.ChatClient.IsVoiceChatActive())
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_AlreadyInVoiceChat, String.Empty);
                return null;
            }

            IVoiceChat voiceChat = chatSession.StartVoiceChat(Dispatcher);
            if (voiceChat != null)
                chatTextBox.AddVoiceChatSentRequest(voiceChat, PrimaryBuddy.DisplayName);
            
            chatStarted = true;

            return voiceChat;
        }

        public void SendFile()
        {
            if (chatSession.IsGroupChat)
                return;

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

                IFileTransfer fileTransfer = chatSession.SendFile(fileName, fileStream);
                fileTransfers.Add(fileTransfer);
                if (fileTransfer != null)
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

        public void DestroySession()
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
                chatSession.TransferInvitationReceived -= new EventHandler<FileTransferInviteEventArgs>(chatSession_TransferInvitationReceived);
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

        public void Show(bool initiatedByUser)
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

        void DeferIfNotLoaded(Action action)
        {
            lock (eventQueue)
                if (!loaded)
                    eventQueue.Enqueue(action);
                else
                    action();
        }

        void UpdateGroupChatControls()
        {
            btnSendFile.IsEnabled = mnuSendFile.IsEnabled = chatSession == null || !chatSession.IsGroupChat;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            txtMessageEditBox.GetFocus();
        }

        bool forceClose;
        public void ForceClose()
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
            IVoiceChat voiceChat = StartVoiceChat();
            ((VoiceChatToolbarControl)sender).VoiceChatContext = voiceChat;
        }
    }
}
