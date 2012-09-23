using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Squiggle.Client;
using Squiggle.Client.Activities;
using Squiggle.Core.Chat.Activity;
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
using Squiggle.Utilities.Threading;

namespace Squiggle.UI.Windows
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : StickyWindowBase, IChatWindow
    {
        static Dictionary<IBuddy, IEnumerable<ChatItem>> chatHistory = new Dictionary<IBuddy, IEnumerable<ChatItem>>();
        
        FlashWindow flash;
        DispatcherTimer statusResetTimer;
        UIActionQueue eventQueue;

        FileTransferCollection fileTransfers = new FileTransferCollection();
        MultiFilter filters = new MultiFilter();
        MultiParser parsers = new MultiParser();

        IChat chatSession;
        ChatState chatState;
        WindowState lastWindowState;
        SquiggleContext context;

        public IBuddy PrimaryBuddy { get; private set; }

        public ChatWindow()
        {
            InitializeComponent();

            chatState = new ChatState();
            eventQueue = new UIActionQueue(Dispatcher);
        }

        internal ChatWindow(IBuddy buddy, SquiggleContext context) : this()
        {
            SetContext(context);
            
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
            this.PrimaryBuddy.Online += new EventHandler(PrimaryBuddy_Online);
            this.PrimaryBuddy.Offline += new EventHandler(PrimaryBuddy_Offline);

            chatTextBox.KeepHistory = !SettingsProvider.Current.Settings.ChatSettings.ClearChatOnWindowClose;

            eventQueue.Enqueue(LoadHistory);

            new ActivitiesMenuHelper(context).LoadActivities(mnuStartActivity, mnuNoActivity, new RelayCommand<IActivity>(StartActivityMenuItem_Click));
        }        

        public IEnumerable<IBuddy> Buddies
        {
            get
            {
                if (chatSession == null)
                    return Enumerable.Repeat(PrimaryBuddy, 1);
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

        void LoadHistory()
        {
            if (IsGroupChat || !chatTextBox.KeepHistory)
                return;

            IEnumerable<ChatItem> history;
            if (chatHistory.TryGetValue(PrimaryBuddy, out history))
                chatTextBox.AddItems(history);
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

        void SetContext(SquiggleContext context)
        {
            this.context = context;

            filters.AddRange(context.PluginLoader.MessageFilters);
            parsers.AddRange(context.PluginLoader.MessageParsers);
        }

        internal void SetChatSession(IChat chat)
        {
            if (chat == null)
                return;

            DestroySession();
            chatSession = chat;
            chatSession.BuzzReceived += new EventHandler<BuddyEventArgs>(chatSession_BuzzReceived);
            chatSession.MessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(chatSession_MessageReceived);
            chatSession.MessageUpdated += new EventHandler<ChatMessageUpdatedEventArgs>(chatSession_MessageUpdated);
            chatSession.BuddyJoined += new EventHandler<BuddyEventArgs>(chatSession_BuddyJoined);
            chatSession.BuddyLeft += new EventHandler<BuddyEventArgs>(chatSession_BuddyLeft);
            chatSession.MessageFailed += new EventHandler<MessageFailedEventArgs>(chatSession_MessageFailed);
            chatSession.BuddyTyping += new EventHandler<BuddyEventArgs>(chatSession_BuddyTyping);
            chatSession.ActivityInvitationReceived += new EventHandler<ActivityInvitationReceivedEventArgs>(chatSession_ActivityInvitationReceived);
            chatSession.GroupChatStarted += new EventHandler(chatSession_GroupChatStarted);
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
                lastWindowState = this.WindowState;

            Dispatcher.Invoke(() =>
            {
                if (this.WindowState != System.Windows.WindowState.Minimized)
                {
                    if (chatState.BuzzPending)
                    {
                        Dispatcher.Delay(() =>
                        {
                            DoBuzzAction();
                        }, TimeSpan.FromSeconds(.5));
                        chatState.BuzzPending = false;
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
            var item = (MessageItem)e.Context;
            if (!e.Updated)
                SendMessage(e.Message);
            else if (item.Message != e.Message)
                UpdateMessage(item.Id, e.Message);
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
            eventQueue.Enqueue(() => OnMessageReceived(e.Sender, e.Id, e.Message, e.FontName, e.Color, e.FontSize, e.FontStyle));
        }

        void chatSession_MessageUpdated(object sender, ChatMessageUpdatedEventArgs e)
        {
            eventQueue.Enqueue(() => OnMessageUpdated(e.Sender, e.Id, e.Message));
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

        void PrimaryBuddy_Online(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!IsGroupChat && Buddies.Contains(PrimaryBuddy))
                    buddyOfflineMessage.Visibility = Visibility.Collapsed;
            });
        }

        void PrimaryBuddy_Offline(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!IsGroupChat && Buddies.Contains(PrimaryBuddy))
                {
                    buddyOfflineMessage.DataContext = PrimaryBuddy.DisplayName;
                    buddyOfflineMessage.Visibility = Visibility.Visible;
                    /* since ZeroMQ doesn't report failure and if the user comes online with new ip/port then existing 
                     * session will continue to talk to old endpoint. Therefore destroy the session */
                    DestroySession(); 
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
            ChangeStatus(String.Format("{0} " + Translation.Instance.ChatWindow_IsTyping, e.Buddy.DisplayName));
            statusResetTimer.Stop();
            statusResetTimer.Start();
        }

        void OnMessageFailed(MessageFailedEventArgs e)
        {
#if DEBUG
            string message = "Following message could not be delivered due to error: " + e.Exception.Coalesce(x=>x.Message, "Unknown");
#else
            string message = Translation.Instance.ChatWindow_MessageCouldNotBeDelivered;
#endif
            string detail = e.Message;
            chatTextBox.AddError(message, detail);
        }

        void OnBuddyJoined(IBuddy buddy)
        {
            Monitor(buddy);
            chatTextBox.AddInfo(String.Format("{0} " + Translation.Instance.ChatWindow_HasJoinedConversation, buddy.DisplayName));
            OnParticipantsChanged();
        }

        void OnBuddyLeft(IBuddy buddy)
        {
            StopMonitoring(buddy);
            chatTextBox.AddInfo(String.Format("{0} " + Translation.Instance.ChatWindow_HasLeftConversation, buddy.DisplayName));
            OnParticipantsChanged();
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

        void Monitor(IBuddy buddy)
        {
            StopMonitoring(buddy);
            buddy.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(buddy_PropertyChanged);
        }

        void MonitorAll()
        {
            foreach (Buddy buddy in Buddies)
                Monitor(buddy);
        }

        void StopMonitoring(IBuddy buddy)
        {
            buddy.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(buddy_PropertyChanged);
        }

        void StopMonitoringAll()
        {
            foreach (Buddy buddy in Buddies)
                StopMonitoring(buddy);
        }

        void OnBuzzReceived(IBuddy buddy)
        {
            if (chatState.CanReceiveBuzz)
            {
                chatTextBox.AddInfo(String.Format("{0} " + Translation.Instance.ChatWindow_HasSentYouBuzz, buddy.DisplayName));
                if (this.WindowState != System.Windows.WindowState.Minimized)
                    DoBuzzAction();
                else
                    chatState.BuzzPending = true;
                FlashWindow();
                chatState.BuzzReceived();
            }
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

        void OnMessageReceived(IBuddy buddy, Guid id, string message, string fontName, System.Drawing.Color color, int fontSize, System.Drawing.FontStyle fontStyle)
        {
            chatState.MessageReceived();
            filters.Filter(message, this, FilterDirection.In, filteredMessage =>
            {
                chatTextBox.AddMessage(id, buddy.DisplayName, filteredMessage, fontName, fontSize, fontStyle, color, parsers, false);

                FlashWindow();
                PlayAlert(AudioAlertType.MessageReceived);
                if (!chatState.ChatStarted && !IsActive)
                    TrayPopup.Instance.Show(Translation.Instance.Popup_NewMessage, String.Format("{0} " + Translation.Instance.Global_ContactSays + ": {1}", buddy.DisplayName, filteredMessage), args => this.Restore());
            });
            ResetStatus();
            chatState.ChatStarted = true;
        }

        void OnMessageUpdated(IBuddy buddy, Guid id, string message)
        {
            chatState.MessageReceived();
            chatTextBox.UpdateMessage(id, message);
            ResetStatus();
        }

        void OnGroupChatStarted()
        {
            MonitorAll();
            OnParticipantsChanged();
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
            chatState.ChatStarted = true;
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
            chatState.ChatStarted = true;
        }

        public void SendMessage(string message)
        {
            chatState.ChatStarted = true;
            if (!EnsureChatSession())
            {
                OnMessageFailed(new MessageFailedEventArgs() { Message = message, Exception = new Exception("User not in buddy list.") });
                return;
            }

            string displayName = context.ChatClient == null ? Translation.Instance.Global_You : context.ChatClient.CurrentUser.DisplayName;
            var settings = SettingsProvider.Current.Settings.PersonalSettings;

            filters.Filter(message, this, FilterDirection.Out, filteredMessage =>
            {
                chatState.LastSentMessageId = Guid.NewGuid();
                chatSession.SendMessage(chatState.LastSentMessageId.Value, settings.FontName, settings.FontSize, settings.FontColor, settings.FontStyle, filteredMessage);
                chatTextBox.AddMessage(chatState.LastSentMessageId.Value, displayName, filteredMessage, settings.FontName, settings.FontSize, settings.FontStyle, settings.FontColor, parsers, true);
            });
        }

        public void UpdateLastMessage(string message)
        {
            if (!chatState.LastSentMessageId.HasValue)
                return;

            UpdateMessage(chatState.LastSentMessageId.Value, message);
        }

        void UpdateMessage(Guid id, string message)
        {
            chatSession.UpdateMessage(id, message);
            chatTextBox.UpdateMessage(id, message);
        }

        public void SendBuzz()
        {
            if (!EnsureChatSession())
                return;

            if (chatState.CanSendBuzz)
            {
                chatTextBox.AddInfo(Translation.Instance.ChatWindow_YouSentBuzz);
                chatSession.SendBuzz();
                chatState.BuzzSent();
                DoBuzzAction();
            }
            else
                chatTextBox.AddError(Translation.Instance.ChatWindow_BuzzTooEarly, String.Empty);
        }

        public IVoiceChatHandler StartVoiceChat()
        {
            if (!EnsureChatSession())
                return null;
            if (chatSession.IsGroupChat)
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_VoiceChatNotAllowedInGroup, String.Empty);
                return null;
            }
            if (context.IsVoiceChatActive)
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_AlreadyInVoiceChat, String.Empty);
                return null;
            }
            if (!context.PluginLoader.HasActivity(SquiggleActivities.VoiceChat))
                return null;

            IActivityExecutor executor = chatSession.CreateActivity();
            IVoiceChatHandler voiceChat = context.PluginLoader.GetActivityHandler(SquiggleActivities.VoiceChat, f => f.CreateInvite(executor, null)) as IVoiceChatHandler;

            if (voiceChat != null)
            {
                voiceChat.Dispatcher = Dispatcher;
                voiceChat.Start();
                chatTextBox.AddVoiceChatSentRequest(context, voiceChat, PrimaryBuddy.DisplayName);
                chatState.ChatStarted = true;
            }
            
            return voiceChat;
        }

        public void SendFile()
        {
            if (!EnsureFileTransferCapibility())
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
            if (!EnsureFileTransferCapibility())
                return;

            foreach (var filePath in filePaths)
                SendFile(filePath);
        }

        bool EnsureFileTransferCapibility()
        {
            if (!context.PluginLoader.HasActivity(SquiggleActivities.FileTransfer))
                return false;

            if (!EnsureChatSession())
                return false;

            if (chatSession.IsGroupChat)
            {
                chatTextBox.AddError(Translation.Instance.ChatWindow_FileTransferNotAllowedInGroup, String.Empty);
                return false;
            }

            return true;
        }

        void SendFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

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
            var args = new Dictionary<string, object>() { { "name", fileName }, { "content", fileStream }, { "size", fileStream.Length } };
            IFileTransferHandler fileTransfer = context.PluginLoader.GetActivityHandler(SquiggleActivities.FileTransfer, f => f.CreateInvite(executor, args)) as IFileTransferHandler;
            if (fileTransfer == null)
                return;

            fileTransfer.Start();
            fileTransfers.Add(fileTransfer);
            chatTextBox.AddFileSentRequest(fileTransfer);

            chatState.ChatStarted = true;
        }

        public void SaveAs()
        {
            string file, format;
            if (ShowSaveDialog(out file, out format))
                SaveTo(file, format);
        }

        public void Save()
        {
            if (String.IsNullOrEmpty(chatState.LastSavedFile))
            {
                string lastSavedFile = chatState.LastSavedFile;
                string lastSavedFormat = chatState.LastSavedFormat;
                
                if (ShowSaveDialog(out lastSavedFile, out lastSavedFormat))
                    Save();

                chatState.LastSavedFormat = lastSavedFormat;
                chatState.LastSavedFile = lastSavedFile;
            }
            else
                SaveTo(chatState.LastSavedFile, chatState.LastSavedFormat);
        }

        public void SaveTo(string fileName, string format)
        {
            chatTextBox.SaveTo(fileName);

            if (format == DataFormats.Rtf)
                return;

            var richTextBox = new System.Windows.Forms.RichTextBox();
            richTextBox.LoadFile(fileName);
            if (format == DataFormats.UnicodeText)
                richTextBox.SaveFile(fileName, System.Windows.Forms.RichTextBoxStreamType.UnicodePlainText);
            else
                richTextBox.SaveFile(fileName, System.Windows.Forms.RichTextBoxStreamType.PlainText);
        }

        public void Invite(IEnumerable<IBuddy> buddies)
        {
            foreach (Buddy buddy in buddies)
                Invite(buddy);
        }

        public void Invite(IBuddy buddy)
        {
            if (buddy != null && chatSession != null && !Buddies.Contains(buddy))
                chatSession.Invite(buddy);
        }

        public void Restore()
        {
            if (Visibility == System.Windows.Visibility.Collapsed)
                Visibility = System.Windows.Visibility.Visible;

            if (WindowState == System.Windows.WindowState.Minimized)
                WindowState = lastWindowState;

            this.Activate();
        }

        public void EndChat()
        {
            DestroySession();
        }

        bool EnsureChatSession()
        {
            if (chatSession != null)
                return true;

            bool sessionReady = false;

            var buddyInList = context.ChatClient.Buddies.FirstOrDefault(b => b.Equals(PrimaryBuddy));
            if (buddyInList != null && context.ChatClient.LoggedIn)
            {
                PrimaryBuddy = buddyInList;
                IChat newSession = context.ChatClient.StartChat(PrimaryBuddy);
                SetChatSession(newSession);
                sessionReady = true;
            }
            
            return sessionReady;
        }

        void DestroySession()
        {
            Dispatcher.Invoke(() =>
            {
                if (voiceController.VoiceChatContext != null)
                    voiceController.VoiceChatContext.Cancel();
            });
            

            fileTransfers.CancelAll();
            if (chatSession != null)
            {
                StopMonitoringAll();

                chatSession.BuzzReceived -= new EventHandler<BuddyEventArgs>(chatSession_BuzzReceived);
                chatSession.MessageReceived -= new EventHandler<ChatMessageReceivedEventArgs>(chatSession_MessageReceived);
                chatSession.MessageUpdated -= new EventHandler<ChatMessageUpdatedEventArgs>(chatSession_MessageUpdated);
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
            if (!chatState.LastMessageReceived.HasValue)
                ChangeStatus(String.Empty);
            else
                ChangeStatus(Translation.Instance.ChatWindow_LastMessageAt + String.Format(" {0:T} on {0:d}", chatState.LastMessageReceived));
        }

        void FlashWindow()
        {
            if (this.Visibility == System.Windows.Visibility.Collapsed)
                Show(false);

            flash.Start();
        }

        internal void Show(bool initiatedByUser)
        {
            if (!initiatedByUser && Visibility == System.Windows.Visibility.Visible)
                return;

            bool activateWindow = initiatedByUser || SettingsProvider.Current.Settings.ChatSettings.StealFocusOnNewMessage;
            
            if (!activateWindow)
                WindowState = WindowState.Minimized;
            Show();
            if (activateWindow)
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

        private void chatTextBox_ItemEdit(object sender, ItemEditEventArgs e)
        {
            if (e.Item is MessageItem)
            {
                txtMessageEditBox.BeginEdit(((MessageItem)e.Item).Message, e.Item);
                txtMessageEditBox.GetFocus();
            }
        }
    }
}
