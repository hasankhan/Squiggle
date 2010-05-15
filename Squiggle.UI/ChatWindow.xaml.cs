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
        IChatSession chatSession;
        Buddy buddy;

        public ChatWindow()
        {
            InitializeComponent();
        }

        public ChatWindow(Buddy buddy, string firstMessage) : this()
        {
            this.buddy = buddy;
            if(!String.IsNullOrEmpty(firstMessage))
                WriteMessage(buddy.DisplayName, firstMessage);
        }

        void chatSession_MessageReceived(object sender, Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs e)
        {
            WriteMessage(buddy.DisplayName, e.Message);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            chatSession.SendMessage(txtMessage.Text);
            WriteMessage("Me", txtMessage.Text);
        }

        private void WriteMessage(string user, string message)
        {
            var title = new Bold(new Run(user+": "));
            var text = new Run(message);
            sentMessages.Inlines.Add(title);
            sentMessages.Inlines.Add(text);
            sentMessages.Inlines.Add(new Run("\r\n"));
            scrollViewer.ScrollToBottom();
            txtMessage.Text = String.Empty;
            txtMessage.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chatSession = this.DataContext as IChatSession;
            chatSession.MessageReceived += new EventHandler<Squiggle.Chat.Services.Chat.Host.MessageReceivedEventArgs>(chatSession_MessageReceived);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }
    }
}
