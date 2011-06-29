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
using Squiggle.Chat;
using Squiggle.UI.Helpers;
using Squiggle.UI.Resources;
using Squiggle.Utilities;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for VoiceChatControl.xaml
    /// </summary>
    public partial class VoiceChatControl : UserControl
    {
        IVoiceChat voiceChat;
        bool sending;

        public string Status { get; private set; }

        public VoiceChatControl()
        {
            InitializeComponent();
        }

        public VoiceChatControl(IVoiceChat voiceChat, bool sending) : this()
        {
            this.voiceChat = voiceChat;
            this.sending = sending;

            this.voiceChat.TransferCancelled += new EventHandler(voiceChat_TransferCancelled);
            this.voiceChat.TransferCompleted += new EventHandler(voiceChat_TransferCompleted);
            this.voiceChat.TransferStarted += new EventHandler(voiceChat_TransferStarted);
            this.voiceChat.TransferFinished += new EventHandler(voiceChat_TransferFinished);

            ShowWaiting();
        }

        void voiceChat_TransferFinished(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowCompleted();
            });
        }

        void voiceChat_TransferStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowAccepted();
            });
        }

        void voiceChat_TransferCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowCompleted();
            }); 
        }

        void voiceChat_TransferCancelled(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowCompleted();
            });
        }

        void ShowAccepted()
        {
            stkAccepted.Visibility = Visibility.Visible;
            stkInvitation.Visibility = Visibility.Hidden;
            stkWaitingAcceptance.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;
        }

        void ShowWaiting()
        {
            stkAccepted.Visibility = Visibility.Hidden;
            stkInvitation.Visibility = sending ? Visibility.Hidden : Visibility.Visible;
            stkWaitingAcceptance.Visibility = sending ? Visibility.Visible : Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;
        }

        void ShowCancelled()
        {
            stkAccepted.Visibility = Visibility.Hidden;
            stkInvitation.Visibility = Visibility.Hidden;
            stkWaitingAcceptance.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;
        }

        void ShowCompleted()
        {
            stkAccepted.Visibility = Visibility.Hidden;
            stkInvitation.Visibility = Visibility.Hidden;
            stkWaitingAcceptance.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Visible;
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            voiceChat.Accept();
            Dispatcher.Invoke(() =>
                {
                    ShowAccepted();
                });
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            voiceChat.Cancel();
            Dispatcher.Invoke(() =>
                {
                    ShowCancelled();
                });
        }

        private void Finished_Click(object sender, RoutedEventArgs e)
        {
            voiceChat.Cancel();
            Dispatcher.Invoke(() =>
            {
                ShowCompleted();
            });
        }
    }
}
