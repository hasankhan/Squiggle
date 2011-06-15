using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Squiggle.Chat.Services.Chat.Host;
using Squiggle.Utilities;
using System.Collections.Generic;
using System.Linq;

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

        IChatHost remoteHost;
        SquiggleEndPoint localUser;
        SquiggleEndPoint remoteUser;
        Stream content;
        Guid transferSessionId;
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

        public FileTransfer(Guid sessionId, IChatHost remoteHost, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, string name, long size, Stream content)
        {
            this.sessionId = sessionId;
            this.localHost = localHost;
            this.remoteUser = remoteUser;
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            this.Name = name;
            this.Size = size;
            this.content = content;
            transferSessionId = Guid.NewGuid();
            sending = true;
        }

        public FileTransfer(Guid sessionId, IChatHost remoteHost, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, string name, long size, Guid transferSessionId)
        {
            this.sessionId = sessionId;
            this.localHost = localHost;
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            this.Name = name;
            this.Size = size;
            this.transferSessionId = transferSessionId;
            sending = false;
            localHost.AppSessionCancelled += new EventHandler<AppSessionEventArgs>(localHost_AppSessionCancelled);
        }

        public void Start()
        {
            localHost.AppInvitationAccepted += new EventHandler<AppSessionEventArgs>(localHost_AppInvitationAccepted);
            localHost.AppSessionCancelled += new EventHandler<AppSessionEventArgs>(localHost_AppSessionCancelled);
            Async.Invoke(() =>
            {
                var data = new FileInviteData() { Name = Name, Size = Size };
                bool success = ExceptionMonster.EatTheException(() => this.remoteHost.ReceiveAppInvite(sessionId, localUser, remoteUser, ChatApps.FileTransfer, transferSessionId, data), "Sending file invite to " + remoteUser.ToString());
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
            localHost.AppDataReceived += new EventHandler<AppDataReceivedEventArgs>(localHost_AppDataReceived);
            bool success = ExceptionMonster.EatTheException(()=>
                            {
                                saveToFile = filePath;
                                content = File.OpenWrite(filePath);
                                remoteHost.AcceptAppInvite(transferSessionId, localUser, remoteUser);
                            }, "accepting file transfer invite from " + remoteUser);
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
                ExceptionMonster.EatTheException(() => this.remoteHost.CancelAppSession(transferSessionId, localUser, remoteUser), "cancelling file transfer with user" + remoteUser);

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
                remoteHost.ReceiveAppData(transferSessionId, localUser, remoteUser, temp);
                bytesRemaining -= bytesRead;
                float progress = (Size - bytesRemaining) / (float)Size * 100;
                worker.ReportProgress((int)progress);
            }

            if (worker.CancellationPending)
                e.Cancel = true;
        }

        void localHost_AppSessionCancelled(object sender, AppSessionEventArgs e)
        {
            if (e.AppSessionId == transferSessionId)
            {
                Cancel(false);
                TransferCancelled(this, EventArgs.Empty);
            }
        }

        void localHost_AppDataReceived(object sender, AppDataReceivedEventArgs e)
        {
            if (e.AppSessionId == transferSessionId && content != null)
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

        void localHost_AppInvitationAccepted(object sender, AppSessionEventArgs e)
        {
            if (e.AppSessionId == transferSessionId)
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
            localHost.AppDataReceived -= new EventHandler<AppDataReceivedEventArgs>(localHost_AppDataReceived);
            localHost.AppInvitationAccepted -= new EventHandler<AppSessionEventArgs>(localHost_AppInvitationAccepted);
            localHost.AppSessionCancelled -= new EventHandler<AppSessionEventArgs>(localHost_AppSessionCancelled);
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
    }
}
