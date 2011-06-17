using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Chat.Host;
using System.IO;
using System.ComponentModel;
using Squiggle.Utilities;

namespace Squiggle.Chat.Services.Chat
{
    abstract class AppHandler
    {
        IChatHost remoteHost;
        SquiggleEndPoint localUser;
        SquiggleEndPoint remoteUser;
        Guid appSessionId;
        ChatHost localHost;
        BackgroundWorker worker;
        Guid sessionId;

        protected const int bufferSize = 32768; // 32KB

        protected bool SelfInitiated { get; private set; }
        protected long BytesReceived { get; private set; }
        protected bool SelfCancelled { get; private set; }
        public bool Connected { get; private set; }

        public event EventHandler<ErrorEventArgs> Error = delegate { };        

        public abstract Guid AppId { get; }

        protected AppHandler(Guid sessionId, IChatHost remoteHost, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser)
        {
            this.sessionId = sessionId;
            this.localHost = localHost;
            this.remoteUser = remoteUser;
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            appSessionId = Guid.NewGuid();
            SelfInitiated = true;
        }

        protected AppHandler(Guid sessionId, IChatHost remoteHost, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, Guid appSessionId)
        {
            this.sessionId = sessionId;
            this.localHost = localHost;
            this.remoteHost = remoteHost;
            this.localUser = localUser;
            this.appSessionId = appSessionId;
            SelfInitiated = false;
            localHost.AppSessionCancelled += new EventHandler<AppSessionEventArgs>(localHost_AppSessionCancelled);
        }

        public void Start()
        {
            localHost.AppInvitationAccepted += new EventHandler<AppSessionEventArgs>(localHost_AppInvitationAccepted);
            localHost.AppSessionCancelled += new EventHandler<AppSessionEventArgs>(localHost_AppSessionCancelled);
            Async.Invoke(() =>
            {
                IEnumerable<KeyValuePair<string, string>> data = CreateInviteMetadata();
                bool success = ExceptionMonster.EatTheException(() => this.remoteHost.ReceiveAppInvite(sessionId, localUser, remoteUser, ChatApps.FileTransfer, appSessionId, data), "Sending app invite to " + remoteUser.ToString());
                if (!success)
                {
                    OnTransferFinished();
                    OnError(new OperationFailedException());
                }
            });
        }

        protected abstract IEnumerable<KeyValuePair<string, string>> CreateInviteMetadata();

        protected void Accept()
        {
            if (SelfInitiated)
                throw new InvalidOperationException("This operation is only valid in context of an invitation.");

            bool success = ExceptionMonster.EatTheException(() =>
            {
                OnAccept();
                remoteHost.AcceptAppInvite(appSessionId, localUser, remoteUser);
            }, "accepting app invite from " + remoteUser);

            if (success)
                OnTransferStarted();
            else
            {
                OnTransferFinished();
                OnError(new OperationFailedException());
            }
        }

        protected virtual void OnAccept() { }

        public void Cancel()
        {
            Cancel(true);
        }

        protected void CompleteTransfer()
        {
            OnTransferFinished();
            OnTransferCompleted();
        }

        protected void Cancel(bool selfCancel)
        {
            SelfCancelled = selfCancel;

            if (selfCancel)
                ExceptionMonster.EatTheException(() => this.remoteHost.CancelAppSession(appSessionId, localUser, remoteUser), "cancelling file transfer with user" + remoteUser);

            if (SelfInitiated && worker != null)
                worker.CancelAsync();
            else
            {
                OnTransferFinished();
                OnTransferCancelled();
            }
        }  

        void localHost_AppSessionCancelled(object sender, AppSessionEventArgs e)
        {
            if (e.AppSessionId == appSessionId)
            {
                Cancel(false);
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
                OnTransferCompleted();
        }

        protected virtual void OnTransferCompleted() { }

        protected void OnError(Exception error)
        {
            Error(this, new ErrorEventArgs(error));
        }

        protected virtual void OnTransferCancelled() { }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnProgressChanged(e.ProgressPercentage);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnTransferStarted();

            TransferData(() => worker.CancellationPending);

            if (worker.CancellationPending)
                e.Cancel = true;
        }

        protected void UpdateProgress(int progress)
        {
            worker.ReportProgress(progress);
        }

        protected abstract void TransferData(Func<bool> cancelPending);

        protected void SendData(byte[] chunk)
        {
            remoteHost.ReceiveAppData(appSessionId, localUser, remoteUser, chunk);
        }

        void localHost_AppDataReceived(object sender, AppDataReceivedEventArgs e)
        {
            if (e.AppSessionId == appSessionId) 
            {
                BytesReceived += e.Chunk.Length;
                OnDataReceived(e.Chunk);
            }
        }

        protected abstract void OnDataReceived(byte[] chunk);

        void localHost_AppInvitationAccepted(object sender, AppSessionEventArgs e)
        {
            if (e.AppSessionId == appSessionId)
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

        protected virtual void OnTransferStarted() 
        {
            Connected = true;
            localHost.AppDataReceived += new EventHandler<AppDataReceivedEventArgs>(localHost_AppDataReceived);
        }

        protected virtual void OnTransferFinished()
        {
            Connected = false;
            localHost.AppDataReceived -= new EventHandler<AppDataReceivedEventArgs>(localHost_AppDataReceived);
            localHost.AppInvitationAccepted -= new EventHandler<AppSessionEventArgs>(localHost_AppInvitationAccepted);
            localHost.AppSessionCancelled -= new EventHandler<AppSessionEventArgs>(localHost_AppSessionCancelled);
        }

        protected virtual void OnProgressChanged(int percentage) { }
    }
}
