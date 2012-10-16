using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Squiggle.UI.Helpers;
using Squiggle.UI.Settings;
using Squiggle.UI.ViewModel;

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
        object editContext;
        EditViewModel editModel;

        public event EventHandler<MessageSendEventArgs> MessageSend = delegate { };
        public event EventHandler MessageTyping = delegate { };

        DateTime? lastTypingNotificationSent;                

        public string Text 
        {
            get { return editModel.Message; }
        }

        public bool CanUndo
        {
            get { return (bool)GetValue(CanUndoProperty); }
            set { SetValue(CanUndoProperty, value); }
        }
        public static readonly DependencyProperty CanUndoProperty = DependencyProperty.Register("CanUndo", typeof(bool), typeof(MessageEditBox), new UIPropertyMetadata(false));

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
            DataContext = editModel = new EditViewModel();

            SettingsProvider.Current.SettingsUpdated += Current_SettingsUpdated;
        }


        public void BeginEdit(string message, object context)
        {
            editModel.Message = message;
            editModel.EditMode = true;
            editContext = context;
        }

        void Current_SettingsUpdated(object sender, EventArgs e)
        {
            SetFont();
        }

        void SetFont()
        {
            var settings = SettingsProvider.Current.Settings.PersonalSettings;
            var result = new FontSetting(settings.FontColor, settings.FontName, settings.FontSize, settings.FontStyle);
            txtMessage.FontFamily = result.Family;
            txtMessage.FontSize = result.Size;
            txtMessage.Foreground = result.Foreground;
            txtMessage.FontWeight = result.Weight;
            txtMessage.FontStyle = result.Style;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            OnMessageSent();            
        }

        void OnMessageSent()
        {
            messages.Add(editModel.Message);
            if (messages.Count > maxMessages)
                messages.RemoveAt(0);
            messageIndex = messages.Count - 1;

            string message = editModel.Message;
            object context = this.editContext;

            editModel.Message = String.Empty;
            txtMessage.Focus();
            ResetEditState();

            MessageSend(this, new MessageSendEventArgs() { Message = message, Context = context });
        }

        private void txtMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            CanUndo = txtMessage.CanUndo;
            if (editModel.Message == String.Empty)
                ResetEditState();
            else
                OnMessageTyping();
            UpdateButtonState();
        }

        void ResetEditState()
        {
            editContext = null;
            editModel.EditMode = false;
        }

        private void UpdateButtonState()
        {
            btnSend.IsEnabled = Enabled && editModel.Message != String.Empty;
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
                        OnMessageSent();
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
                editModel.Message = messages[messageIndex];
        }

        void OnMessageTyping()
        {
            if (!lastTypingNotificationSent.HasValue || DateTime.Now.Subtract(lastTypingNotificationSent.Value).TotalSeconds > 5)
            {
                MessageTyping(this, new EventArgs());
                lastTypingNotificationSent = DateTime.Now;
            }
        }

        public void GetFocus()
        {
            txtMessage.Focus();
        }
    }

    class EditViewModel : ViewModelBase
    {
        string message;
        bool editMode;

        public string Message
        {
            get { return message; }
            set { Set(() => Message, ref message, value); }
        }

        public bool EditMode
        {
            get { return editMode; }
            set { Set(() => EditMode, ref editMode, value); }
        }
    }

    public class MessageSendEventArgs : EventArgs
    {
        public bool Updated
        {
            get { return Context != null; }
        }
        public object Context { get; set; }
        public string Message { get; set; }
    }

    public class FileDroppedEventArgs : EventArgs
    {
        public string[] Files { get; set; }
    }
}
