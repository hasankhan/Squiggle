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
using System.Diagnostics;
using Squiggle.UI.Settings;

namespace Squiggle.UI.Controls
{   
    /// <summary>
    /// Interaction logic for MessageEditBox.xaml
    /// </summary>
    public partial class MessageEditBox : UserControl
    {
        const int maxMessages = 10;
        int messageIndex = 0;
        List<string> messages = new List<string>();

        public event EventHandler<FileDroppedEventArgs> FileDropped = delegate { };
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
            SetFont();

            SettingsProvider.Current.SettingsUpdated += new EventHandler(Current_SettingsUpdated);
        }

        void Current_SettingsUpdated(object sender, EventArgs e)
        {
            SetFont();
        }

        private void SetFont()
        {
            var settings = SettingsProvider.Current.Settings.PersonalSettings;
            txtMessage.FontFamily = new System.Windows.Media.FontFamily(settings.Font.FontFamily.Name);
            System.Drawing.Color color = settings.FontColor;
            txtMessage.FontSize = settings.FontSize;
            txtMessage.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            RaiseMessageSendEvent();            
        }

        public void GetFocus()
        {
            txtMessage.Focus();
        }

        private void RaiseMessageSendEvent()
        {
            messages.Add(txtMessage.Text);
            if (messages.Count > maxMessages)
                messages.RemoveAt(0);
            messageIndex = messages.Count - 1;

            MessageSend(this, new MessageSendEventArgs() { Message = txtMessage.Text });
            txtMessage.Text = String.Empty;
            txtMessage.Focus();
        }

        private void txtMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtMessage.Text != String.Empty)
                NotifyTyping();
            btnSend.IsEnabled = txtMessage.Text != String.Empty;
        }        

        private void txtMessage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Up)
            {
                e.Handled = true;
                ShowMessage();
                messageIndex--;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Down)
            {
                e.Handled = true;
                ShowMessage();
                messageIndex++;
            }
            else if (e.Key == Key.Enter)
            {
                if (!(Keyboard.Modifiers == ModifierKeys.Control || Keyboard.Modifiers == ModifierKeys.Shift))
                {

                    if (btnSend.IsEnabled)
                        RaiseMessageSendEvent();
                    e.Handled = true;
                }
            }
        }

        private void ShowMessage()
        {
            if (messageIndex < 0)
                messageIndex = 0;
            if (messageIndex >= messages.Count)
                messageIndex = messages.Count - 1;

            if (messages.Any())
                txtMessage.Text = messages[messageIndex];
        }

        void NotifyTyping()
        {
            if (!lastTypingNotificationSent.HasValue || DateTime.Now.Subtract(lastTypingNotificationSent.Value).TotalSeconds > 5)
            {
                MessageTyping(this, new EventArgs());
                lastTypingNotificationSent = DateTime.Now;
            }
        }

        private void txtMessage_PreviewDrag(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
                e.Handled = true;
            }
        }

        private void txtMessage_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                    FileDropped(this, new FileDroppedEventArgs() { Files = files });
                e.Handled = true;
            }
        }
    }

    public class MessageSendEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class FileDroppedEventArgs : EventArgs
    {
        public string[] Files { get; set; }
    }
}
