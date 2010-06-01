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
        string firstMessage;
 
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

        public ChatWindow(Buddy buddy, string firstMessage) : this()
        {
            this.buddy = buddy;
            this.firstMessage = firstMessage;            
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown += new KeyEventHandler(ChatWindow_KeyDown);
            chatSession = this.DataContext as IChat;
            chatSession.MessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(chatSession_MessageReceived);
            chatSession.BuddyJoined += new EventHandler<BuddyEventArgs>(chatSession_BuddyJoined);
            chatSession.BuddyLeft += new EventHandler<BuddyEventArgs>(chatSession_BuddyLeft);
            chatSession.MessageFailed += new EventHandler<MessageFailedEventArgs>(chatSession_MessageFailed);
            chatSession.BuddyTyping += new EventHandler<BuddyEventArgs>(chatSession_BuddyTyping);
            chatSession.TransferInvitationReceived += new EventHandler<FileTransferInviteEventArgs>(chatSession_TransferInvitationReceived);
            if (!String.IsNullOrEmpty(firstMessage))
                OnMessageReceived(buddy, firstMessage);
        }

        void chatSession_TransferInvitationReceived(object sender, FileTransferInviteEventArgs e)
        {
            chatTextBox.AddFileTransfer(e.Sender.DisplayName, e.Invitation);
        }

        void chatSession_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            OnMessageReceived(e.Sender, e.Message);
        }        

        void ChatWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        void chatSession_BuddyTyping(object sender, BuddyEventArgs e)
        {
            OnBuddyTyping(e);
        }

        private void OnBuddyTyping(BuddyEventArgs e)
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

        void chatSession_MessageFailed(object sender, MessageFailedEventArgs e)
        {
            OnMessageFailed(e);
        }

        private void OnMessageFailed(MessageFailedEventArgs e)
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

        void chatSession_BuddyLeft(object sender, BuddyEventArgs e)
        {
            OnBuddyLeft(e);
        }

        private void OnBuddyLeft(BuddyEventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(new Action(() => OnBuddyLeft(e)));
            else
            {
                txtUserLeftMessage.Text = e.Buddy.DisplayName + " has left the chat.";
                txtUserLeftMessage.Visibility = Visibility.Visible;
            }
        }

        void chatSession_BuddyJoined(object sender, BuddyEventArgs e)
        {
            OnBuddyJoined();          
        }

        void OnBuddyJoined()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(new Action(OnBuddyJoined));
            else
                txtUserLeftMessage.Visibility = Visibility.Hidden;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            chatSession.Leave();
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
                if (!this.IsActive)
                    flash.Start();
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

       
    }
}
