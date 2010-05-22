using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for MessageEditBox.xaml
    /// </summary>
    public partial class MessageEditBox : UserControl
    {

        public event EventHandler<MessageSendEventArgs> MessageSend = delegate { };
        public event EventHandler MessageTyping = delegate { };

        DateTime? lastTypingNotificationSent;

        public string Text 
        {
            get
            {
                return txtMessage.Text;
            }
        }

        public MessageEditBox()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            RaiseMessageSendEvent();            
        }

        private void RaiseMessageSendEvent()
        {
            MessageSend(this, new MessageSendEventArgs() { Message = txtMessage.Text });
            txtMessage.Text = String.Empty;
            txtMessage.Focus();
        }

        private void txtMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtMessage.Text != String.Empty)
                if (!lastTypingNotificationSent.HasValue || DateTime.Now.Subtract(lastTypingNotificationSent.Value).TotalSeconds > 5)
                    MessageTyping(this, new EventArgs());
        }

        private void txtMessage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control || Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    txtMessage.Text += "\r\n";
                    txtMessage.CaretIndex = txtMessage.Text.Length;
                }
                else 
                {
                    RaiseMessageSendEvent();
                    e.Handled = true;
                }
               
              }
        }
    }

    public class MessageSendEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
