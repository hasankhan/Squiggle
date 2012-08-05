using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Squiggle.Chat;
using Squiggle.UI.Helpers;
using Squiggle.UI.Resources;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.Activities;
using Squiggle.UI.Components;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for FileTransferControl.xaml
    /// </summary>
    public partial class FileTransferControl : UserControl, INotifyPropertyChanged
    {
        IFileTransferHandler fileTransfer;
        bool sending;

        string downloadFolder;

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public long FileSize { get; private set; }
        public string Status { get; private set; }


        public string DownloadFolder
        {
            get { return downloadFolder; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentException("Download folder can not be empty string.");
                else
                    downloadFolder = value;
            }
        }

        public FileTransferControl()
        {
            DownloadFolder = System.IO.Path.Combine(AppInfo.Location, "Downloads");
            InitializeComponent();
        }

        public FileTransferControl(IFileTransferHandler fileTransfer, bool sending) : this()
        {
            this.fileTransfer = fileTransfer;
            this.sending = sending;

            FileName = fileTransfer.Name;
            FileSize = fileTransfer.Size;
            Status = sending ? Translation.Instance.FileTransfer_Waiting : ToReadableFileSize(FileSize);
            btnCancelTransfer.Content = sending ? Translation.Instance.FileTransfer_Cancel : Translation.Instance.FileTransfer_Reject;

            NotifyPropertyChanged();

            this.fileTransfer.ProgressChanged += new EventHandler<System.ComponentModel.ProgressChangedEventArgs>(fileTransfer_ProgressChanged);
            this.fileTransfer.TransferStarted += new EventHandler(fileTransfer_TransferStarted); 
            this.fileTransfer.TransferCancelled += new EventHandler(fileTransfer_TransferCancelled);
            this.fileTransfer.TransferCompleted += new EventHandler(fileTransfer_TransferCompleted);

            ShowWaiting();
        }

        void fileTransfer_TransferCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Status = sending ? Translation.Instance.FileTransfer_FileSent : Translation.Instance.FileTransfer_FileReceived;
                NotifyPropertyChanged();

                ShowCompleted();
            });
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (Shell.CreateDirectoryIfNotExists(DownloadFolder))
            {
                string filePath = Shell.GetUniqueFilePath(DownloadFolder, fileTransfer.Name);
                AcceptDownload(filePath);
            }
        }        

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.SaveFileDialog())
            {
                dlg.FileName = fileTransfer.Name;
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    AcceptDownload(dlg.FileName);
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                CancelDownload(true);
            });

        }

        void fileTransfer_TransferCancelled(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                CancelDownload(false);
            });
        }

        void fileTransfer_TransferStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowDownloading();
            });
        }

        void fileTransfer_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progress.Value = e.ProgressPercentage;
            });
        }

        private void CancelDownload(bool selfCancel)
        {
            ShowCancelled();

            Status = sending ? Translation.Instance.FileTransfer_SendingCancelled : Translation.Instance.FileTransfer_Cancelled;
            NotifyPropertyChanged();

            if (selfCancel)
                fileTransfer.Cancel();
        }

        private void AcceptDownload(string filePath)
        {
            FilePath = filePath;
            NotifyPropertyChanged();

            ShowDownloading();

            fileTransfer.Accept(filePath);
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

        private void ShowDownloading()
        {
            stkAccepted.Visibility = Visibility.Visible;
            stkInvitation.Visibility = Visibility.Hidden;
            stkWaitingAcceptance.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;

            Status = sending ? Translation.Instance.FileTransfer_Sending : Translation.Instance.FileTransfer_Receiving;
            NotifyPropertyChanged();
        }

        private void NotifyPropertyChanged()
        {
            OnPropertyChanged("FileName");
            OnPropertyChanged("FileSize");
            OnPropertyChanged("Status");
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Shell.OpenFile(FilePath);
        }

        private void ShowInFolder_Click(object sender, RoutedEventArgs e)
        {
            string file = DataContext as string;
            Shell.ShowInFolder(FilePath);
        }

        static string ToReadableFileSize(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return String.Format("{0:##.##} {1}", Decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }
    }
}
