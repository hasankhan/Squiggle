using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Chat.Services.Chat
{
    class FileTransfer: IFileTransfer
    {
        public event EventHandler TransferCompleted = delegate { };
        public event EventHandler TransferStarted = delegate { };
        public event EventHandler TransferCancelled = delegate { };
        public event EventHandler TransferFinished = delegate { };
        public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> ProgressChanged = delegate { };
        public event EventHandler<ErrorEventArgs> Error = delegate { };        

        IChatHost remoteUser;
        IPEndPoint localUser;
        Stream content;
        Guid id;
        ChatHost localHost;
        BackgroundWorker worker;
        bool sending;
        long bytesReceived;
        bool selfCancelled;
        string saveToFile;
        Guid sessionId;
        const int bufferSize = 32768; // 32KB

        public long Size { get; private set; }
        public string Name { get; private set; }

        public FileTransfer(Guid sessionId, IChatHost remoteHost, ChatHost localHost, IPEndPoint localUser, string name, long size, Stream content)
        {
            this.sessionId = sessionId;
            this.localHost = localHost;
            this.remoteUser = remoteHost;
            this.localUser = localUser;
            this.Name = name;
            this.Size = size;
            this.content = content;
            id = Guid.NewGuid();
            sending = true;
        }

        public FileTransfer(Guid sessionId, IChatHost remoteHost, ChatHost localHost, IPEndPoint localUser, string name, long size, Guid id)
        {
            this.sessionId = sessionId;
            this.localHost = localHost;
            this.remoteUser = remoteHost;
            this.localUser = localUser;
            this.Name = name;
            this.Size = size;
            this.id = id;
            sending = false;
            localHost.TransferCancelled += new EventHandler<FileTransferEventArgs>(localHost_TransferCancelled);
        }

        public void Start()
        {
            localHost.InvitationAccepted += new EventHandler<FileTransferEventArgs>(localHost_InvitationAccepted);
            localHost.TransferCancelled += new EventHandler<FileTransferEventArgs>(localHost_TransferCancelled);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                bool success = L(() => this.remoteUser.ReceiveFileInvite(sessionId, localUser, id, Name, Size));
                if (!success)
                {
                    OnTransferFinished();
                    OnError(new OperationFailedException());
                }
            });
        }

        public void Accept(string filePath)
        {
            if (sending)
                throw new InvalidOperationException("This operation is only valid in context of an invitation.");
            localHost.TransferDataReceived += new EventHandler<FileTransferDataReceivedEventArgs>(localHost_TransferDataReceived);
            bool success = L(()=>
                            {
                                saveToFile = filePath;
                                content = File.OpenWrite(filePath);
                                remoteUser.AcceptFileInvite(id);
                            });
            if (success)
                OnTransferStarted();
            else
            {
                OnTransferFinished();
                OnError(new OperationFailedException());
            }
        }
        
        public void Cancel()
        {
            Cancel(true);
        }

        void Cancel(bool selfCancel)
        {
            selfCancelled = selfCancel;

            if (selfCancel)
                L(() => this.remoteUser.CancelFileTransfer(id));

            if (sending && worker!=null)            
                 worker.CancelAsync();
            else
            {
                OnTransferFinished();
                OnTransferCancelled();
            }
        }        

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnTransferFinished();

            if (e.Cancelled)
                OnTransferCancelled();
            else if (e.Error != null)
                OnError(e.Error);
            else
                TransferCompleted(this, EventArgs.Empty);
        }

        void OnError(Exception error)
        {
            Error(this, new ErrorEventArgs(error));
        }

        void OnTransferCancelled()
        {
            if (!sending && content != null)
                File.Delete(saveToFile);
            if (selfCancelled)
                TransferCancelled(this, EventArgs.Empty);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgress(e.ProgressPercentage);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnTransferStarted();

            byte[] buffer = new byte[bufferSize];
            long bytesRemaining = Size;
            while (bytesRemaining > 0 && !worker.CancellationPending)
            {
                int bytesRead = content.Read(buffer, 0, buffer.Length);
                byte[] temp = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, temp, 0, temp.Length);
                remoteUser.ReceiveFileContent(id, temp);
                bytesRemaining -= bytesRead;
                float progress = (Size - bytesRemaining) / (float)Size * 100;
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
            if (e.ID == id && content != null)
            {
                bytesReceived += e.Chunk.Length;
                content.Write(e.Chunk, 0, e.Chunk.Length);

                float progress = bytesReceived / (float)Size * 100;
                UpdateProgress((int)progress);

                if (bytesReceived >= Size)
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
            TransferStarted(this, EventArgs.Empty);
        }        

        void OnTransferFinished()
        {
            localHost.TransferDataReceived -= new EventHandler<FileTransferDataReceivedEventArgs>(localHost_TransferDataReceived);
            localHost.InvitationAccepted -= new EventHandler<FileTransferEventArgs>(localHost_InvitationAccepted);
            localHost.TransferCancelled -= new EventHandler<FileTransferEventArgs>(localHost_TransferCancelled);
            if (content != null)
            {
                content.Dispose();
                content = null;
            }
            TransferFinished(this, EventArgs.Empty);
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
