using System;
using System.Windows;
using System.Windows.Input;
using Squiggle.Chat;
using Squiggle.UI.Controls;
using System.Windows.Threading;
using System.IO;
using Squiggle.UI.Settings;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        IChat chatSession;
        Buddy buddy;
        FlashWindow flash;
        DateTime? lastMessageReceived;
        DispatcherTimer statusResetTimer;
        EventQueue eventQueue = new EventQueue();
        bool loaded;
        string lastSavedFile;
        string lastSavedFormat;

        public ChatWindow()
        {
            InitializeComponent();
            flash = new FlashWindow(this);

            statusResetTimer = new DispatcherTimer();
            statusResetTimer.Interval = TimeSpan.FromSeconds(5);
            statusResetTimer.Tick += (sender, e) => ResetStatus();
            this.Activated += new EventHandler(ChatWindow_Activated);
            this.StateChanged += new EventHandler(ChatWindow_StateChanged);

            SettingsProvider.Current.SettingsUpdated += (sender, e) => LoadSettings();
            LoadSettings();
        }

        void LoadSettings()
        {
            editMessageBox.txtMessage.SpellCheck.IsEnabled = SettingsProvider.Current.Settings.GeneralSettings.SpellCheck;
        }

        public ChatWindow(Buddy buddy, IChat chatSession) : this()
        {
            ChatSession = chatSession;
            this.buddy = buddy;            
        }

        public IChat ChatSession
        {
            get { return chatSession; }
            set
            {
                chatSession = value;
                this.DataContext = value;
                chatSession.MessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(chatSession_MessageReceived);
                chatSession.BuddyJoined += new EventHandler<BuddyEventArgs>(chatSession_BuddyJoined);
                chatSession.BuddyLeft += new EventHandler<BuddyEventArgs>(chatSession_BuddyLeft);
                chatSession.MessageFailed += new EventHandler<MessageFailedEventArgs>(chatSession_MessageFailed);
                chatSession.BuddyTyping += new EventHandler<BuddyEventArgs>(chatSession_BuddyTyping);
                chatSession.TransferInvitationReceived += new EventHandler<FileTransferInviteEventArgs>(chatSession_TransferInvitationReceived);
            }
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown += new KeyEventHandler(ChatWindow_KeyDown);
            loaded = true;
            eventQueue.DequeueAll();
        }

        void ChatWindow_Activated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                editMessageBox.GetFocus();
            }));
        }

        void ChatWindow_StateChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                editMessageBox.GetFocus();
            }));
        }

        void ChatWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }       

        private void Window_Closed(object sender, EventArgs e)
        {
            chatSession.Leave();
        }

        void OnTransferInvite(FileTransferInviteEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                string downloadsFolder = SettingsProvider.Current.Settings.GeneralSettings.DownloadsFolder;
                chatTextBox.AddFileReceiveRequest(e.Sender.DisplayName, e.Invitation, downloadsFolder);
                FlashWindow();
            });
        }

        void OnMessageReceived(Buddy buddy, string message)
        {
            Dispatcher.Invoke(() =>
            {
                lastMessageReceived = DateTime.Now;
                chatTextBox.AddMessage(buddy.DisplayName, message);
                ResetStatus();
                FlashWindow();
            });
        }        

        private void txtMessage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    e.Handled = true;
        }

        private void editMessageBox_MessageSend(object sender, MessageSendEventArgs e)
        {
            chatSession.SendMessage(e.Message);
            chatTextBox.AddMessage("Me", e.Message);
        }

        private void editMessageBox_MessageTyping(object sender, EventArgs e)
        {
            chatSession.NotifyTyping();
        }

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            SendFile();
        }

        public void SendFile()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SendFile(dialog.FileName);
            }
        }

        public void SendFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileStream = File.OpenRead(filePath);
                string fileName = Path.GetFileName(filePath);
                IFileTransfer fileTransfer = chatSession.SendFile(fileName, fileStream);
                chatTextBox.AddFileSentRequest(fileTransfer);
            }
        }

        void chatSession_TransferInvitationReceived(object sender, FileTransferInviteEventArgs e)
        {
            if (!loaded)
            {
                eventQueue.Enqueue(sender, e, chatSession_TransferInvitationReceived);
                return;
            }
            OnTransferInvite(e);
        }        

        void chatSession_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            if (!loaded)
            {
                eventQueue.Enqueue(sender, e, chatSession_MessageReceived);
                return;
            }
            OnMessageReceived(e.Sender, e.Message);
        }

        void chatSession_BuddyTyping(object sender, BuddyEventArgs e)
        {
            if (!loaded)
            {
                eventQueue.Enqueue(sender, e, chatSession_BuddyTyping);
                return;
            }
            OnBuddyTyping(e);
        }

        void chatSession_MessageFailed(object sender, MessageFailedEventArgs e)
        {
            if (!loaded)
            {
                eventQueue.Enqueue(sender, e, chatSession_MessageFailed);
                return;
            }
            OnMessageFailed(e);
        }

        void chatSession_BuddyLeft(object sender, BuddyEventArgs e)
        {
            if (!loaded)
            {
                eventQueue.Enqueue(sender, e, chatSession_BuddyLeft);
                return;
            }
            OnBuddyLeft(e);
        }

        void chatSession_BuddyJoined(object sender, BuddyEventArgs e)
        {
            if (!loaded)
            {
                eventQueue.Enqueue(sender, e, chatSession_BuddyJoined);
                return;
            }
            OnBuddyJoined();
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
                string message = "Following message could not be delivered";
#endif
                string detail = e.Message;
                chatTextBox.AddError(message, detail);
            });
        }

        void OnBuddyLeft(BuddyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtUserLeftMessage.Text = e.Buddy.DisplayName + " has left the chat.";
                txtUserLeftMessage.Visibility = Visibility.Visible;
            });
        }

        void OnBuddyJoined()
        {
            Dispatcher.Invoke(() => txtUserLeftMessage.Visibility = Visibility.Hidden);
        }               

        void ResetStatus()
        {
            statusResetTimer.Stop();
            if (!lastMessageReceived.HasValue)
                ChangeStatus(String.Empty);
            else
                ChangeStatus("Last message received at " + String.Format("{0:T} on {0:d}", lastMessageReceived));
        }

        void ChangeStatus(string message, params object[] args)
        {
            txbStatus.Text = String.Format(message, args);
        }

        void FlashWindow()
        {
            if (!this.IsActive)
                flash.Start();
        }

        private void editMessageBox_FileDropped(object sender, FileDroppedEventArgs e)
        {
            foreach (string file in e.Files)
                SendFile(file);
        }

        private void OpenReceivedFilesMenu_Click(object sender, RoutedEventArgs e)
        {
            SquiggleUtility.OpenDownloadsFolder();
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

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            SquiggleUtility.ShowSettingsDialog(this);
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            SquiggleUtility.ShowAboutDialog();
        }

        private void SendFileMenu_Click(object sender, RoutedEventArgs e)
        {
            SendFile();
        }
    }
}
