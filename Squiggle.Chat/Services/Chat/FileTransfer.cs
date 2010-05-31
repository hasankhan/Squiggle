using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using Squiggle.Chat.Services.Chat.Host;
using System.ComponentModel;
using System.Diagnostics;

namespace Squiggle.Chat.Services.Chat
{
    class FileTransfer: IFileTransfer
    {
        public event EventHandler TransferCompleted = delegate { };
        public event EventHandler TransferStarted = delegate { };
        public event EventHandler TransferCancelled = delegate { };
        public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> ProgressChanged = delegate { };
        public event EventHandler<ErrorEventArgs> Error = delegate { };        

        IChatHost remoteUser;
        IPEndPoint localUser;        
        int size;
        Stream content;
        Guid id;
        ChatHost localHost;
        BackgroundWorker worker;
        bool sending;
        int bytesReceived;

        public string Name { get; private set; }

        public FileTransfer(IChatHost remoteHost, ChatHost localHost, IPEndPoint localUser, string name, int size, Stream content)
        {
            this.localHost = localHost;
            this.remoteUser = remoteHost;
            this.localUser = localUser;
            this.Name = name;
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
            this.Name = name;
            this.size = size;
            this.id = id;
            sending = false;
        }

        public void Start()
        {
            localHost.InvitationAccepted += new EventHandler<FileTransferEventArgs>(localHost_InvitationAccepted);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                bool success = L(() => this.remoteUser.ReceiveFileInvite(localUser, id, Name, size));
                if (!success)
                    OnTransferFinished();
            });
        }

        public void Accept(string filePath)
        {
            if (sending)
                throw new InvalidOperationException("This operation is only valid in context of an invitation.");
            localHost.TransferDataReceived += new EventHandler<FileTransferDataReceivedEventArgs>(localHost_TransferDataReceived);
            bool success = L(()=>
                            {
                                content = File.OpenWrite(filePath);
                                remoteUser.AcceptFileInvite(id);
                            });
            if (success)
                OnTransferStarted();
            else
                OnTransferFinished();
        }
        
        public void Cancel()
        {
            Cancel(true);
        }

        void Cancel(bool notifyOther)
        {
            if (sending)
                worker.CancelAsync();
            else
            {
                OnTransferFinished();
                TransferCancelled(this, EventArgs.Empty);
            }
            if (notifyOther)
                L(() => this.remoteUser.CancelFileTransfer(id));
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

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgress(e.ProgressPercentage);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnTransferStarted();

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

        void localHost_TransferCancelled(object sender, FileTransferEventArgs e)
        {
            if (e.ID == id)
            {
                Cancel(false);
                TransferCancelled(this, EventArgs.Empty);
            }
        }

        void localHost_TransferDataReceived(object sender, FileTransferDataReceivedEventArgs e)
        {
            if (e.ID == id)
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
        }

        void localHost_InvitationAccepted(object sender, FileTransferEventArgs e)
        {
            if (e.ID == id)
            {
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
        }

        void OnTransferStarted()
        {
            localHost.TransferCancelled += new EventHandler<FileTransferEventArgs>(localHost_TransferCancelled);
            TransferStarted(this, EventArgs.Empty);
        }        

        void OnTransferFinished()
        {
            localHost.TransferDataReceived -= new EventHandler<FileTransferDataReceivedEventArgs>(localHost_TransferDataReceived);
            localHost.InvitationAccepted -= new EventHandler<FileTransferEventArgs>(localHost_InvitationAccepted);
            localHost.TransferCancelled -= new EventHandler<FileTransferEventArgs>(localHost_TransferCancelled);
            content.Dispose();
        }

        void UpdateProgress(int percentage)
        {
            ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
        }

        bool L(Action action)
        {
            bool success = true;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                success = false;
                Trace.WriteLine(ex.Message);
            }
            return success;
        }
    }
}
