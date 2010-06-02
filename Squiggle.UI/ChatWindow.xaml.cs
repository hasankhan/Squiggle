using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Squiggle.Chat;
using Squiggle.UI.Controls;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;

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
        
        public ChatWindow()
        {
            InitializeComponent();
            flash = new FlashWindow(this);

            statusResetTimer = new DispatcherTimer();
            statusResetTimer.Interval = TimeSpan.FromSeconds(5);
            statusResetTimer.Tick += (sender, e) => ResetStatus();
            this.Activated += new EventHandler(ChatWindow_Activated);
            this.StateChanged += new EventHandler(ChatWindow_StateChanged);
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
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(new Action(() => OnTransferInvite(e)));
            else
            {
                chatTextBox.AddFileReceiveRequest(e.Sender.DisplayName, e.Invitation);
                FlashWindow();
            }
        }

        void OnMessageReceived(Buddy buddy, string message)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(new Action(() => OnMessageReceived(buddy, message)));
            else
            {
                lastMessageReceived = DateTime.Now;
                chatTextBox.AddMessage(buddy.DisplayName, message);
                ResetStatus();
                FlashWindow();
            }
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
            using (var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (dlg.CheckFileExists)
                    {
                        FileInfo file = new FileInfo(dlg.FileName);
                        FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                        int size = Convert.ToInt32(Decimal.Divide(file.Length, 1024));
                        IFileTransfer fileTransfer = chatSession.SendFile(file.Name, size, fileStream);
                        chatTextBox.AddFileSentRequest(fileTransfer);
                    }
                }
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
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(new Action(() => OnBuddyTyping(e)));
            else
            {
                ChangeStatus(String.Format("{0} is typing...", e.Buddy.DisplayName));
                statusResetTimer.Stop();
                statusResetTimer.Start();
            }
        }        

        void OnMessageFailed(MessageFailedEventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(new Action(() => OnMessageFailed(e)));
            else
            {
                string message = "Following message could not be sent due to error: " + e.Exception.Message;
                string detail = e.Message;
                chatTextBox.AddError(message, detail);
            }
        }

        void OnBuddyLeft(BuddyEventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(new Action(() => OnBuddyLeft(e)));
            else
            {
                txtUserLeftMessage.Text = e.Buddy.DisplayName + " has left the chat.";
                txtUserLeftMessage.Visibility = Visibility.Visible;
            }
        }

        void OnBuddyJoined()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(new Action(OnBuddyJoined));
            else
                txtUserLeftMessage.Visibility = Visibility.Hidden;
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
    }
}
