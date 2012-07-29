using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Squiggle.Chat;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.Chat.Apps.Voice;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for VoiceChatToolbarControl.xaml
    /// </summary>
    public partial class VoiceChatToolbarControl : UserControl, INotifyPropertyChanged
    {
        IVoiceChat voiceChatContext;

        public event EventHandler StartChat = delegate { };

        public VoiceChatToolbarControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public IVoiceChat VoiceChatContext 
        {
            get { return voiceChatContext; }
            set 
            {
                if (voiceChatContext != null)
                {
                    voiceChatContext.TransferCancelled -= new EventHandler(voiceChatContext_TransferCancelled);
                    voiceChatContext.TransferCompleted -= new EventHandler(voiceChatContext_TransferCompleted);
                    voiceChatContext.TransferFinished -= new EventHandler(voiceChatContext_TransferFinished);
                    voiceChatContext.TransferStarted -= new EventHandler(voiceChatContext_TransferStarted);
                }
                voiceChatContext = value;
                if (voiceChatContext != null)
                {
                    voiceChatContext.TransferCancelled += new EventHandler(voiceChatContext_TransferCancelled);
                    voiceChatContext.TransferCompleted += new EventHandler(voiceChatContext_TransferCompleted);
                    voiceChatContext.TransferFinished += new EventHandler(voiceChatContext_TransferFinished);
                    voiceChatContext.TransferStarted += new EventHandler(voiceChatContext_TransferStarted);
                }

                OnPropertyChanged("VoiceChatContext"); 
            }
        }

        void voiceChatContext_TransferStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                volume.GetBindingExpression(Slider.ValueProperty).UpdateTarget();
            });
        }

        void voiceChatContext_TransferFinished(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                EndChat();
            });
        }

        void voiceChatContext_TransferCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                EndChat();
            });
        }

        void voiceChatContext_TransferCancelled(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                EndChat();
            });
        }

        private void StartVoiceChat_Click(object sender, RoutedEventArgs e)
        {
            StartChat(this, e);
        }

        private void StopVoiceChat_Click(object sender, RoutedEventArgs e)
        {
            if (voiceChatContext != null)
                voiceChatContext.Cancel();
        }

        public void EndChat()
        {
            VoiceChatContext = null;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
