using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Utilities;

namespace Squiggle.Core.Chat
{
    public abstract class AppHandler: IAppHandler
    {
        BackgroundWorker worker;
        AppSession session;

        protected const int bufferSize = 32768; // 32KB

        protected long BytesReceived { get; private set; }
        protected bool SelfCancelled { get; private set; }
        public bool IsConnected { get; private set; }

        public event EventHandler<ErrorEventArgs> Error = delegate { };
        public event EventHandler TransferCompleted = delegate { };
        public event EventHandler TransferStarted = delegate { };
        public event EventHandler TransferCancelled = delegate { };
        public event EventHandler TransferFinished = delegate { };
        public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> ProgressChanged = delegate { };

        public abstract Guid AppId { get; }

        protected bool SelfInitiated
        {
            get { return session.SelfInitiated; }
        }

        protected AppHandler(AppSession session)
        {
            this.session = session;

            if (!session.SelfInitiated)
                session.ChatHost.AppSessionCancelled += new EventHandler<AppSessionEventArgs>(chatHost_AppSessionCancelled);
        }

        public void Start()
        {
            session.ChatHost.AppInvitationAccepted += new EventHandler<AppSessionEventArgs>(localHost_AppInvitationAccepted);
            session.ChatHost.AppSessionCancelled += new EventHandler<AppSessionEventArgs>(chatHost_AppSessionCancelled);
            Async.Invoke(() =>
            {
                IEnumerable<KeyValuePair<string, string>> metadata = CreateInviteMetadata();
                bool success = session.SendInvite(AppId, metadata);
                if (!success)
                {
                    OnTransferFinished();
                    OnError(new OperationFailedException());
                }
            });
        }

        public void Cancel()
        {
            Cancel(true);
        }

        protected abstract IEnumerable<KeyValuePair<string, string>> CreateInviteMetadata();

        protected void Accept()
        {
            if (session.SelfInitiated)
                throw new InvalidOperationException("This operation is only valid in context of an invitation.");

            OnAccept();
            bool success = session.Accept();

            if (success)
                OnTransferStarted();
            else
            {
                OnTransferFinished();
                OnError(new OperationFailedException());
            }
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
                session.Cancel();

            if (session.SelfInitiated && worker != null)
                worker.CancelAsync();
            else
            {
                OnTransferFinished();
                OnTransferCancelled();
            }
        }  

        void chatHost_AppSessionCancelled(object sender, AppSessionEventArgs e)
        {
            if (e.AppSessionId == session.Id)
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

        protected virtual void OnAccept() { }
 
        protected virtual void OnTransferCompleted() 
        {
            TransferCompleted(this, EventArgs.Empty);
        }

        protected void OnError(Exception error)
        {
            Error(this, new ErrorEventArgs(error));
        }

        protected virtual void OnTransferCancelled()
        {
            TransferCancelled(this, EventArgs.Empty);
        }

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
            if (!IsConnected)
                return;

            session.SendData(chunk, OnError);
        }

        void chatHost_AppDataReceived(object sender, AppDataReceivedEventArgs e)
        {
            if (e.AppSessionId == session.Id) 
            {
                BytesReceived += e.Chunk.Length;
                OnDataReceived(e.Chunk);
            }
        }

        protected abstract void OnDataReceived(byte[] chunk);

        void localHost_AppInvitationAccepted(object sender, AppSessionEventArgs e)
        {
            if (e.AppSessionId == session.Id)
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
            IsConnected = true;
            session.ChatHost.AppDataReceived += new EventHandler<AppDataReceivedEventArgs>(chatHost_AppDataReceived);
            TransferStarted(this, EventArgs.Empty);
        }

        protected virtual void OnTransferFinished()
        {
            IsConnected = false;
            session.ChatHost.AppDataReceived -= new EventHandler<AppDataReceivedEventArgs>(chatHost_AppDataReceived);
            session.ChatHost.AppInvitationAccepted -= new EventHandler<AppSessionEventArgs>(localHost_AppInvitationAccepted);
            session.ChatHost.AppSessionCancelled -= new EventHandler<AppSessionEventArgs>(chatHost_AppSessionCancelled);
            TransferFinished(this, EventArgs.Empty);
        }

        protected virtual void OnProgressChanged(int percentage) 
        {
            ProgressChanged(this, new ProgressChangedEventArgs(percentage, null)); 
        }
    }
}
