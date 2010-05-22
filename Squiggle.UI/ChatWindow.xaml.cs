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

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        IChat chatSession;
        Buddy buddy;
        FlashForm flash;

        public ChatWindow()
        {
            InitializeComponent();
            flash = new FlashForm(this);

            this.Activated += new EventHandler(ChatWindow_Activated);
        }

        void ChatWindow_Activated(object sender, EventArgs e)
        {
            flash.Stop();
        }

        public ChatWindow(Buddy buddy, string firstMessage) : this()
        {
            this.buddy = buddy;
            if(!String.IsNullOrEmpty(firstMessage))
                OnMessageReceived(buddy, firstMessage);
        }        

        void chatSession_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            OnMessageReceived(e.Sender, e.Message);
        }

        private void WriteMessage(string user, string message)
        {
            var title = new Bold(new Run(user+": "));
            var text = new Run(message);
            sentMessages.Inlines.Add(title);
            sentMessages.Inlines.Add(text);
            sentMessages.Inlines.Add(new Run("\r\n"));
            scrollViewer.ScrollToBottom();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chatSession = this.DataContext as IChat;
            chatSession.MessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(chatSession_MessageReceived);
            chatSession.BuddyJoined += new EventHandler<BuddyEventArgs>(chatSession_BuddyJoined);
            chatSession.BuddyLeft += new EventHandler<BuddyEventArgs>(chatSession_BuddyLeft);
            chatSession.MessageFailed += new EventHandler<MessageFailedEventArgs>(chatSession_MessageFailed);
            chatSession.BuddyTyping += new EventHandler<BuddyEventArgs>(chatSession_BuddyTyping);
        }

        void chatSession_BuddyTyping(object sender, BuddyEventArgs e)
        {
            txbLastMessageReceived.Text = String.Format("{0} is typing", e.Buddy.DisplayName);
        }

        void chatSession_MessageFailed(object sender, MessageFailedEventArgs e)
        {
            var text = new Run("Following message could not be sent due to error: " + e.Exception.Message);
            sentMessages.Inlines.Add(text);
            sentMessages.Inlines.Add(new Run("\r\n\t"));
            sentMessages.Inlines.Add(e.Message);
            scrollViewer.ScrollToBottom();
        }

        void chatSession_BuddyLeft(object sender, BuddyEventArgs e)
        {
            txtUserLeftMessage.Text = e.Buddy.DisplayName + " has left the chat.";
            txtUserLeftMessage.Visibility = Visibility.Visible;
        }

        void chatSession_BuddyJoined(object sender, BuddyEventArgs e)
        {
            txtUserLeftMessage.Visibility = Visibility.Hidden;            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            chatSession.Leave();
        }

        void OnMessageReceived(Buddy buddy, string message)
        {
            WriteMessage(buddy.DisplayName, message);
            txbLastMessageReceived.Text = String.Format("Last message received at " + String.Format("{0:T} on {0:d}", DateTime.Now));
            if (!this.IsActive)
                flash.Start();
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
            WriteMessage("Me", e.Message);
        }

        private void editMessageBox_MessageTyping(object sender, EventArgs e)
        {
            chatSession.NotifyTyping();
        }
    }
}
