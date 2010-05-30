using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using Squiggle.Chat.Services.Chat.Host;
using System.ComponentModel;

namespace Squiggle.Chat.Services.Chat
{
    class FileTransfer: IFileTransfer
    {
        #region IFileTransfer Members

        public event EventHandler TransferCompleted = delegate { };
        public event EventHandler TransferStarted = delegate { };
        public event EventHandler TransferCancelled = delegate { };
        public event EventHandler<ChunkReceivedEventArgs> ChunkReceived = delegate { };
        public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> ProgressChanged = delegate { };
        public event EventHandler<ErrorEventArgs> Error = delegate { };

        public void Cancel()
        {
            if (sending)
                worker.CancelAsync();
            else
            {
                OnTransferFinished();
                TransferCancelled(this, EventArgs.Empty);
            }
        }

        #endregion

        IChatHost remoteUser;
        IPEndPoint localUser;
        string name;
        int size;
        Stream content;
        Guid id;
        ChatHost localHost;
        BackgroundWorker worker;
        bool sending;
        int bytesReceived;

        public FileTransfer(IChatHost remoteHost, ChatHost localHost, IPEndPoint localUser, string name, int size, Stream content)
        {
            this.localHost = localHost;
            this.remoteUser = remoteHost;
            this.localUser = localUser;
            this.name = name;
            this.size = size;
            this.content = content;
            id = Guid.NewGuid();
            sending = true;
        }

        public FileTransfer(IChatHost remoteHost, ChatHost localHost, IPEndPoint localUser, string name, int size, Guid id)
        {
            this.localHost = localHost;
            this.remoteUser = remoteHost;
            this.localUser = localUser;
            this.name = name;
            this.size = size;
            this.id = id;
            sending = false;
        }

        public void Start()
        {
            localHost.InvitationAccepted += new EventHandler<FileTransferEventArgs>(localHost_InvitationAccepted);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    this.remoteUser.ReceiveFileInvite(localUser, id, name, size);
                }
                catch (Exception ex)
                {
                    Error(this, new ErrorEventArgs(ex));
                }
            });
        }

        public void Accept(string filePath)
        {
            if (sending)
                throw new InvalidOperationException("This operation is only valid in context of an invitation.");
            content = File.OpenWrite(filePath);
            localHost.TransferDataReceived += new EventHandler<FileTransferDataReceivedEventArgs>(localHost_TransferDataReceived);
            remoteUser.AcceptFileInvite(id);
        }

        void localHost_TransferDataReceived(object sender, FileTransferDataReceivedEventArgs e)
        {
            bytesReceived += e.Chunk.Length;
            content.Write(e.Chunk, 0, e.Chunk.Length);

            float progress = bytesReceived / (float)size * 100;
            UpdateProgress((int)progress);

            if (bytesReceived == size)
            {
                OnTransferFinished();
                TransferCompleted(this, EventArgs.Empty);
            }
        }

        void localHost_InvitationAccepted(object sender, FileTransferEventArgs e)
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnTransferFinished();

            if (e.Cancelled)
                TransferCancelled(this, EventArgs.Empty);
            else if (e.Error != null)
                Error(this, new ErrorEventArgs(e.Error));
            else
                TransferCompleted(this, EventArgs.Empty);
        }

        void OnTransferFinished()
        {
            localHost.TransferDataReceived -= new EventHandler<FileTransferDataReceivedEventArgs>(localHost_TransferDataReceived);
            localHost.InvitationAccepted -= new EventHandler<FileTransferEventArgs>(localHost_InvitationAccepted);
            content.Dispose();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgress(e.ProgressPercentage);
        }

        void UpdateProgress(int percentage)
        {
            ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            TransferStarted(this, EventArgs.Empty);

            byte[] buffer = new byte[1024];
            int bytesRemaining = size;
            while (bytesRemaining > 0 && !worker.CancellationPending)
            {
                int bytesRead = content.Read(buffer, 0, buffer.Length);
                remoteUser.ReceiveFileContent(id, buffer);
                bytesRemaining -= bytesRead;
                float progress = (size - bytesRemaining) / (float)size * 100;
                worker.ReportProgress((int)progress);
            }

            if (worker.CancellationPending)
                e.Cancel = true;
        }        
    }
}
