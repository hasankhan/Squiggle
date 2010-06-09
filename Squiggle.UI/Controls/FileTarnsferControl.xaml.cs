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
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for FileTransferControl.xaml
    /// </summary>
    public partial class FileTarnsferControl : UserControl, INotifyPropertyChanged
    {
        IFileTransfer fileTransfer;
        bool sending;

        string downloadFolder;

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public int FileSize { get; private set; }
        public string Status { get; private set; }


        public string DownloadFolder
        {
            get { return downloadFolder; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentException("Download folder can not be empty string");
                else
                    downloadFolder = value;
            }
        }

        public FileTarnsferControl()
        {
            DownloadFolder = "Downloads";
            InitializeComponent();
        }

        public FileTarnsferControl(IFileTransfer fileTransfer, bool sending) : this()
        {
            this.fileTransfer = fileTransfer;
            this.sending = sending;

            FileName = fileTransfer.Name;
            FileSize = fileTransfer.Size;
            Status = sending ? "Waiting" : FileSize.ToReadableFileSize();
            btnCancelTransfer.Content = sending ? "Cancel" : "Reject";

            NotifyPropertyChanged();

            this.fileTransfer.ProgressChanged += new EventHandler<System.ComponentModel.ProgressChangedEventArgs>(fileTransfer_ProgressChanged);
            this.fileTransfer.TransferStarted += new EventHandler(fileTransfer_TransferStarted); 
            this.fileTransfer.TransferCancelled += new EventHandler(fileTransfer_TransferCancelled);
            this.fileTransfer.TransferCompleted += new EventHandler(fileTransfer_TransferCompleted);

            ShowWaiting();
        }

        void fileTransfer_TransferCompleted(object sender, EventArgs e)
        {
            Status = sending ? "File Sent" : "File Received";
            NotifyPropertyChanged();

            ShowCompleted();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(DownloadFolder))
                Directory.CreateDirectory(DownloadFolder);

            string filePath = GetUniqueFilePath(DownloadFolder, fileTransfer.Name);         

            AcceptDownload(filePath);
        }        

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    AcceptDownload(dlg.SelectedPath);
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
            progress.Value = e.ProgressPercentage;            
        }

        private void CancelDownload(bool selfCancel)
        {
            ShowCancelled();

            Status = sending ? "Sending Cancelled" : "Cancelled";
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

            Status = sending ? "Sending" : "Receiving";
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
            if (File.Exists(FilePath))
                Process.Start(new ProcessStartInfo(FilePath));
        }

        private void ShowInFolder_Click(object sender, RoutedEventArgs e)
        {
            string file = DataContext as string;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "explorer.exe";
            startInfo.Arguments = "/select,\"" + FilePath + "\"";

            Process.Start(startInfo);
        }

        static string GetUniqueFilePath(string downloadFolder, string originalFileName)
        {
            string extension = System.IO.Path.GetExtension(originalFileName);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(originalFileName);

            string filePath = System.IO.Path.Combine(downloadFolder, originalFileName);
            for (int i = 1; File.Exists(filePath); i++)
            {
                string temp = String.Format("{0}({1}){2}", fileName, i, extension);
                filePath = System.IO.Path.Combine(downloadFolder, temp);
            }
            return filePath;
        }
    }
}
