using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Squiggle.UI.Settings;
using Squiggle.UI.Helpers;

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

        public bool CanUndo
        {
            get { return (bool)GetValue(CanUndoProperty); }
            set { SetValue(CanUndoProperty, value); }
        }

        public static readonly DependencyProperty CanUndoProperty =
            DependencyProperty.Register("CanUndo", typeof(bool), typeof(MessageEditBox), new UIPropertyMetadata(false));

        public bool Enabled
        {
            get { return txtMessage.IsEnabled; }
            set
            {
                txtMessage.IsEnabled = value;
                UpdateButtonState();
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
            var settings = SquiggleUtility.GetFontSettings();
            txtMessage.FontFamily = settings.Family;
            txtMessage.FontSize = settings.Size;
            txtMessage.Foreground = settings.Foreground;
            txtMessage.FontWeight = settings.Weight;
            txtMessage.FontStyle = settings.Style;
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

            string message = txtMessage.Text;
            txtMessage.Text = String.Empty;
            txtMessage.Focus();

            MessageSend(this, new MessageSendEventArgs() { Message =  message});
        }

        private void txtMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            CanUndo = txtMessage.CanUndo;
            if (txtMessage.Text != String.Empty)
                NotifyTyping();
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            btnSend.IsEnabled = Enabled && txtMessage.Text != String.Empty;
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
