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

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for FileSendControl.xaml
    /// </summary>
    public partial class FileSendControl : UserControl
    {
        IFileTransfer fileTransfer;

        public FileSendControl()
        {
            InitializeComponent();
        }

        public FileSendControl(IFileTransfer fileTransfer) : this()
        {
            this.fileTransfer = fileTransfer;
            this.fileTransfer.TransferCancelled += new EventHandler(fileTransfer_TransferCancelled);
            this.fileTransfer.TransferCompleted += new EventHandler(fileTransfer_TransferCompleted);
            this.fileTransfer.TransferStarted += new EventHandler(fileTransfer_TransferStarted);
            this.fileTransfer.ProgressChanged += new EventHandler<System.ComponentModel.ProgressChangedEventArgs>(fileTransfer_ProgressChanged);
        }

        void fileTransfer_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progress.Value = e.ProgressPercentage;
            });
        }  

        void fileTransfer_TransferStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                stkCancelled.Visibility = Visibility.Visible;
                stkAccepted.Visibility = Visibility.Hidden;
                stkCompleted.Visibility = Visibility.Hidden;
            });
        }

        void fileTransfer_TransferCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                stkCompleted.Visibility = Visibility.Visible;
                stkCancelled.Visibility = Visibility.Hidden;
                stkAccepted.Visibility = Visibility.Hidden;
            });
        }

        void fileTransfer_TransferCancelled(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => CancelDownload());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDownload();
        }

        void CancelDownload()
        {
            stkCancelled.Visibility = Visibility.Visible;
            stkAccepted.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;

            fileTransfer.Cancel();
        }
    }
}
