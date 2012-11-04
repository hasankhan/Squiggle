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
        string buddyName;
        string activityName;

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

        public GenericActivityControl(IActivityHandler session, string buddyName, string activityName, bool sending): this()
        {
            this.session = session;
            this.sending = sending;
            this.buddyName = buddyName;
            this.activityName = activityName;

            Status = String.Format(sending ? Translation.Instance.Activity_Waiting : Translation.Instance.Activity_Invitation, buddyName, activityName);
            btnCancelTransfer.Content = sending ? Translation.Instance.Activity_Cancel : Translation.Instance.Activity_Reject;

            session.TransferStarted += session_TransferStarted;
            session.TransferCancelled += session_TransferCancelled;
            session.TransferCompleted += session_TransferCompleted;

            ShowWaiting();
        }

        void session_TransferCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Status = String.Format(Translation.Instance.Activity_Completed, activityName);

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
                ShowStarted();
            });
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Accept();
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

            Status = String.Format(Translation.Instance.Activity_Cancelled, activityName);

            if (selfCancel)
                session.Cancel();
        }

        void Accept()
        {
            ShowStarted();

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

        void ShowStarted()
        {
            stkAccepted.Visibility = Visibility.Visible;
            stkInvitation.Visibility = Visibility.Hidden;
            stkWaitingAcceptance.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;

            Status = String.Format(Translation.Instance.Activity_Started, activityName);
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
