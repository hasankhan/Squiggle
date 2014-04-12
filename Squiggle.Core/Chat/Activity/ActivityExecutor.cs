using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;

namespace Squiggle.Core.Chat.Activity
{
    class ActivityExecutor: IActivityExecutor
    {
        BackgroundWorker worker;
        ActivitySession session;
        ActivityHandler handler;
        Guid activityId;

        public long BytesReceived { get; private set; }
        public bool SelfCancelled { get; private set; }
        public bool IsConnected { get; private set; }

        public bool SelfInitiated
        {
            get { return session.SelfInitiated; }
        }

        public ActivityExecutor(Guid activityId, ActivitySession session)
        {
            this.activityId = activityId;
            this.session = session as ActivitySession;
            if (this.session == null)
                throw new ArgumentException("Invalid session object", "session");

            if (!this.session.SelfInitiated)
                this.session.ChatHost.ActivitySessionCancelled += chatHost_ActivitySessionCancelled;
        }

        public void SetHandler(ActivityHandler handler)
        {
            this.handler = handler;
        }

        public void Start()
        {
            session.ChatHost.ActivityInvitationAccepted += chatHost_ActivityInvitationAccepted;
            session.ChatHost.ActivitySessionCancelled += chatHost_ActivitySessionCancelled;
            Task.Run(() =>
            {
                IEnumerable<KeyValuePair<string, string>> metadata = handler.CreateInviteMetadata();
                bool success = session.SendInvite(activityId, metadata);
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

        public void Accept()
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

        public void CompleteTransfer()
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

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnProgressChanged(e.ProgressPercentage);
        }        

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnTransferStarted();

            handler.TransferData(() => worker.CancellationPending);

            if (worker.CancellationPending)
                e.Cancel = true;
        }

        public void UpdateProgress(int progress)
        {
            worker.ReportProgress(progress);
        }

        public void SendData(byte[] chunk)
        {
            if (!IsConnected)
                return;

            session.SendData(chunk, OnError);
        }

        void chatHost_ActivitySessionCancelled(object sender, ActivitySessionEventArgs e)
        {
            if (e.ActivitySessionId == session.Id)
            {
                Cancel(false);
                OnTransferCancelled();
            }
        }        

        void chatHost_ActivityDataReceived(object sender, ActivityDataReceivedEventArgs e)
        {
            if (e.ActivitySessionId == session.Id) 
            {
                BytesReceived += e.Chunk.Length;
                OnDataReceived(e.Chunk);
            }
        }        

        void chatHost_ActivityInvitationAccepted(object sender, ActivitySessionEventArgs e)
        {
            if (e.ActivitySessionId == session.Id)
            {
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                worker.RunWorkerAsync();
            }
        }

        void OnTransferStarted() 
        {
            IsConnected = true;
            session.ChatHost.ActivityDataReceived += chatHost_ActivityDataReceived;
            handler.OnTransferStarted();
        }

        void OnTransferFinished()
        {
            IsConnected = false;
            session.ChatHost.ActivityDataReceived -= chatHost_ActivityDataReceived;
            session.ChatHost.ActivityInvitationAccepted -= chatHost_ActivityInvitationAccepted;
            session.ChatHost.ActivitySessionCancelled -= chatHost_ActivitySessionCancelled;
            handler.OnTransferFinished();
        }

        void OnDataReceived(byte[] chunk)
        {
            handler.OnDataReceived(chunk);
        }

        void OnTransferCancelled()
        {
            handler.OnTransferCancelled();
        }

        void OnTransferCompleted()
        {
            handler.OnTransferCompleted();
        }

        void OnError(Exception ex)
        {
            handler.OnError(ex);
        }

        void OnProgressChanged(int percentage)
        {
            handler.OnProgressChanged(percentage);
        }

        void OnAccept()
        {
            handler.OnAccept();
        }
    }
}
