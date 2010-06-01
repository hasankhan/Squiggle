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

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for FileTransferControl.xaml
    /// </summary>
    public partial class FileTransferControl : UserControl, INotifyPropertyChanged
    {
        IFileTransfer fileTransfer;
        string downloadFolder;

        public int Size { get { return fileTransfer.Size; } }
        public string FileName { get; private set; }
        public int FileSize { get; set; }

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

        public FileTransferControl()
        {
            DownloadFolder = "Downloads";
            InitializeComponent();
        }

        public FileTransferControl(IFileTransfer fileTransfer) : this()
        {
            this.fileTransfer = fileTransfer;

            FileName = fileTransfer.Name;
            FileSize = fileTransfer.Size;
            NotifyPropertyChanged();

            this.fileTransfer.ProgressChanged += new EventHandler<System.ComponentModel.ProgressChangedEventArgs>(fileTransfer_ProgressChanged);
        }

        

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(DownloadFolder))
                Directory.CreateDirectory(DownloadFolder);

            string filePath = System.IO.Path.Combine(DownloadFolder, fileTransfer.Name);
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
            RejectDownload();
        }

        void fileTransfer_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
            if (e.ProgressPercentage == 100)
            {
                stkCompleted.Visibility = Visibility.Visible;
                stkCancelled.Visibility = Visibility.Hidden;
                stkAccepted.Visibility = Visibility.Hidden;
                stkInvitation.Visibility = Visibility.Hidden;
            }
        }

        private void RejectDownload()
        {
            stkCancelled.Visibility = Visibility.Visible;
            stkAccepted.Visibility = Visibility.Hidden;
            stkInvitation.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;

            fileTransfer.Cancel();
        }

        private void AcceptDownload(string filePath)
        {
            stkAccepted.Visibility = Visibility.Visible;
            stkCancelled.Visibility = Visibility.Hidden;
            stkInvitation.Visibility = Visibility.Hidden;
            stkCompleted.Visibility = Visibility.Hidden;

            fileTransfer.Accept(filePath);
        }

        private void NotifyPropertyChanged()
        {
            OnPropertyChanged("FileName");
            OnPropertyChanged("FileSize");
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
