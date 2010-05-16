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
                WriteMessage(buddy.DisplayName, firstMessage);
        }

        void chatSession_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            WriteMessage(e.Sender.DisplayName, e.Message);
            if (!this.IsActive)
                flash.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            chatSession.SendMessage(txtMessage.Text);
            WriteMessage("Me", txtMessage.Text);
            txtMessage.Text = String.Empty;
            txtMessage.Focus();
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
    }
}
