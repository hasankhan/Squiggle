using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Squiggle.Chat;
using System.Linq;
using Squiggle.UI.Controls;
using Squiggle.UI.Settings;
using System.Collections.Generic;
using Squiggle.UI.MessageParsers;
using Squiggle.UI.Helpers;
using Squiggle.UI.MessageFilters;
using System.Text;
using System.Globalization;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
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

        MultiFilter filters = new MultiFilter();
        bool chatStarted;

        public Buddy PrimaryBuddy { get; private set; }

        public ChatWindow()
        {
            InitializeComponent();

            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                LayoutRoot.FlowDirection = System.Windows.FlowDirection.RightToLeft;

            this.Height = Properties.Settings.Default.ChatWindowHeight;
            this.Width = Properties.Settings.Default.ChatWindowWidth;

            flash = new FlashWindow(this);

            statusResetTimer = new DispatcherTimer();
            statusResetTimer.Interval = TimeSpan.FromSeconds(5);
            statusResetTimer.Tick += (sender, e) => ResetStatus();
            this.Activated += new EventHandler(ChatWindow_Activated);
            this.StateChanged += new EventHandler(ChatWindow_StateChanged);

            SettingsProvider.Current.SettingsUpdated += (sender, e) => LoadSettings();
            LoadSettings();

            filters.Add(AliasFilter.Instance);
            filters.Add(CommandsFilter.Instance);
        }

        public ChatWindow(Buddy buddy) : this()
        {
            this.PrimaryBuddy = buddy;
            this.PrimaryBuddy.Online += new EventHandler(buddy_Online);
            this.PrimaryBuddy.Offline += new EventHandler(buddy_Offline);
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
            chatTextBox.MessageParsers.Remove(EmoticonParser.Instance);
            if (SettingsProvider.Current.Settings.GeneralSettings.ShowEmoticons)
                chatTextBox.MessageParsers.Add(EmoticonParser.Instance);

            txtMessageEditBox.txtMessage.SpellCheck.IsEnabled = SettingsProvider.Current.Settings.GeneralSettings.SpellCheck;
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
            chatSession.GroupChatStarted += new EventHandler(chatSession_GroupChatStarted);
            txtMessageEditBox.Enabled = true;
            mnuInviteContact.IsEnabled = !IsBroadcastChat;
            UpdateTitle();
            MonitorAll(); 
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGroupChatControls();
            this.KeyDown += new KeyEventHandler(ChatWindow_KeyDown);
            lock (eventQueue)
            {
                loaded = true;
                eventQueue.DequeueAll();
            }
        }

        void ChatWindow_Activated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtMessageEditBox.GetFocus();
            });
        }

        void ChatWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Minimized)
                lastState = this.WindowState;

            Dispatcher.Invoke(() =>
            {
                if (this.WindowState != System.Windows.WindowState.Minimized)
                {
                    txtMessageEditBox.GetFocus();
                    if (buzzPending)
                    {
                        Async.Invoke(()=>
                        {
                            Dispatcher.Invoke(DoBuzzAction);
                        }, TimeSpan.FromSeconds(.5));
                        buzzPending = false;
                    }
                }
            });
        }

        void ChatWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }       

        private void Window_Closed(object sender, EventArgs e)
        {
            DestroySession();
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

        private void txtMessageEditBox_FileDropped(object sender, FileDroppedEventArgs e)
        {
            foreach (string file in e.Files)
                SendFiles(file);
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
            DeferIfNotLoaded(()=>OnTransferInvite(e));
        }

        void chatSession_BuzzReceived(object sender, BuddyEventArgs e)
        {
            DeferIfNotLoaded(()=>OnBuzzReceived(e.Buddy));
        }           

        void chatSession_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            DeferIfNotLoaded(()=>OnMessageReceived(e.Sender, e.Message, e.FontName, e.Color, e.FontSize, e.FontStyle));
        }

        void chatSession_BuddyTyping(object sender, BuddyEventArgs e)
        {
            DeferIfNotLoaded(()=>OnBuddyTyping(e));
        }

        void chatSession_MessageFailed(object sender, MessageFailedEventArgs e)
        {
            DeferIfNotLoaded(()=>OnMessageFailed(e));
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
            Dispatcher.Invoke(UpdateTitle);
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
                ChangeStatus(String.Format("{0} is typing...", e.Buddy.DisplayName));
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
                string message = "Following message could not be delivered to some of the recepients: ";
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
                UpdateGroupChatControls();
                chatTextBox.AddInfo(String.Format("{0} has joined the conversation.", buddy.DisplayName));
                UpdateTitle();
            });
        }        

        void OnBuddyLeft(Buddy buddy)
        {
            StopMonitoring(buddy);
            Dispatcher.Invoke(() =>
            {
                UpdateGroupChatControls();
                chatTextBox.AddInfo(String.Format("{0} has left the conversation.", buddy.DisplayName));
                UpdateTitle();
            });
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
                    chatTextBox.AddInfo(String.Format("{0} sent you a buzz.", buddy.DisplayName));
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

        void OnMessageReceived(Buddy buddy, string message, string fontName, System.Drawing.Color color, int fontSize, System.Drawing.FontStyle fontStyle)
        {
            Dispatcher.Invoke(() =>
            {                
                lastMessageReceived = DateTime.Now;
                chatTextBox.AddMessage(buddy.DisplayName, message, fontName, fontSize, fontStyle, color);
                ResetStatus();
                FlashWindow();
                if (this.WindowState == System.Windows.WindowState.Minimized && !chatStarted)
                    TrayPopup.Instance.Show("New Message", String.Format("{0} says: {1}", buddy.DisplayName, message), args=>this.Restore());
                AudioAlert.Instance.Play(AudioAlertType.MessageReceived);
            });
            chatStarted = true;
        }

        void OnGroupChatStarted()
        {
            Dispatcher.Invoke(()=>
            {
                UpdateGroupChatControls();
                MonitorAll();
                UpdateTitle();
            });
        }        

        void OnTransferInvite(FileTransferInviteEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                string downloadsFolder = SettingsProvider.Current.Settings.GeneralSettings.DownloadsFolder;
                chatTextBox.AddFileReceiveRequest(e.Sender.DisplayName, e.Invitation, downloadsFolder);
                fileTransfers.Add(e.Invitation);
                FlashWindow();
            });
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
  
            string displayName = MainWindow.Instance.ChatClient == null ? "You" : MainWindow.Instance.ChatClient.CurrentUser.DisplayName;
            var settings = SettingsProvider.Current.Settings.PersonalSettings;

            var temp = new StringBuilder(message);
            if (filters.Filter(temp, this))
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
                chatTextBox.AddInfo("You have sent a buzz.");
                chatSession.SendBuzz();
                lastBuzzSent = DateTime.Now;
                DoBuzzAction();
            }
            else
                chatTextBox.AddError("Buzz can not be sent too frequently.", String.Empty);
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
                    chatTextBox.AddError(String.Format("Could not read file '{0}'. Please make sure its not in use.", fileName), null);
                    return;
                }

                IFileTransfer fileTransfer = chatSession.SendFile(fileName, fileStream);
                fileTransfers.Add(fileTransfer);
                if (fileTransfer != null)
                    chatTextBox.AddFileSentRequest(fileTransfer);
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

        public void Invite(Buddy buddy)
        {
            if (buddy != null && chatSession != null)
                chatSession.Invite(buddy);
        }

        public void Restore()
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                WindowState = lastState;
            this.Activate();
        }

        public void DestroySession()
        {
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
                    txtMessageEditBox.Enabled = false;
                    UpdateTitle();
                });
            }
        }                 

        void UpdateTitle()
        {
            if (IsBroadcastChat)
                this.Title = "Broadcast chat";
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
                ChangeStatus("Last message received at " + String.Format("{0:T} on {0:d}", lastMessageReceived));
        }

        void FlashWindow()
        {
            flash.Start();
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
            IEnumerable<Buddy> buddies = SquiggleUtility.SelectContacts("Invite someone to this conversation.", this, b=>Buddies.Contains(b));
            foreach (Buddy buddy in buddies)
                Invite(buddy);
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
    }
}
