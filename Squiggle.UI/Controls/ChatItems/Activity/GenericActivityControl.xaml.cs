using Squiggle.Core.Chat.Activity;
using Squiggle.UI.Resources;
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
using Squiggle.Utilities.Threading;

namespace Squiggle.UI.Controls.ChatItems.Activity
{
    /// <summary>
    /// Interaction logic for GenericActivityControl.xaml
    /// </summary>
    public partial class GenericActivityControl : UserControl, INotifyPropertyChanged
    {
        IActivityHandler session;
        bool sending;

        string _status;
        public string Status 
        {
            get { return _status; }
            set 
            { 
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public GenericActivityControl()
        {
            InitializeComponent();
        }

        public GenericActivityControl(IActivityHandler session, bool sending)
        {
            this.session = session;
            this.sending = sending;

            Status = sending ? Translation.Instance.FileTransfer_Waiting : String.Empty;
            btnCancelTransfer.Content = sending ? Translation.Instance.FileTransfer_Cancel : Translation.Instance.FileTransfer_Reject;

            session.TransferStarted += session_TransferStarted;
            session.TransferCancelled += session_TransferCancelled;
            session.TransferCompleted += session_TransferCompleted;

            ShowWaiting();
        }

        void session_TransferCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Status = sending ? Translation.Instance.FileTransfer_FileSent : Translation.Instance.FileTransfer_FileReceived;

                ShowCompleted();
            });
        }

        void session_TransferCancelled(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                CancelDownload(false);
            });
        }

        void session_TransferStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowDownloading();
            });
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            AcceptDownload();
        } 

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                CancelDownload(true);
            });
        }

        void CancelDownload(bool selfCancel)
        {
            ShowCancelled();

            Status = sending ? Translation.Instance.FileTransfer_SendingCancelled : Translation.Instance.FileTransfer_Cancelled;

            if (selfCancel)
                session.Cancel();
        }

        void AcceptDownload()
        {
            ShowDownloading();

            session.Start();
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
            stkCompleted.Visibility = sending ? Visibility.Hidden : Visibility.Visible;
        }

        void ShowWaiting()
        {
            stkAccepted.Visibility = Visibility.Hidden;
            stkInvitation.Visibility = sending ? Visibility.Hidden : Visibility.Visible;
            stkWaitingAcceptance.Visibility = sending ? Visibility.Visible : Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;
        }

        void ShowDownloading()
        {
            stkAccepted.Visibility = Visibility.Visible;
            stkInvitation.Visibility = Visibility.Hidden;
            stkWaitingAcceptance.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;

            Status = sending ? Translation.Instance.FileTransfer_Sending : Translation.Instance.FileTransfer_Receiving;
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
