using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;

namespace Squiggle.Core.Chat.Activity
{
    class ActivityExecutor: IActivityExecutor
    {
        CancellationTokenSource? _cts;
        IProgress<int>? _progress;
        ActivitySession session;
        ActivityHandler handler = null!;
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

            if (session.SelfInitiated && _cts != null)
                _cts.Cancel();
            else
            {
                OnTransferFinished();
                OnTransferCancelled();
            }
        }  

        public void UpdateProgress(int progress)
        {
            _progress?.Report(progress);
        }

        public void SendData(byte[] chunk)
        {
            if (!IsConnected)
                return;

            session.SendData(chunk, OnError);
        }

        void chatHost_ActivitySessionCancelled(object? sender, ActivitySessionEventArgs e)
        {
            if (e.ActivitySessionId == session.Id)
            {
                Cancel(false);
                OnTransferCancelled();
            }
        }        

        void chatHost_ActivityDataReceived(object? sender, ActivityDataReceivedEventArgs e)
        {
            if (e.ActivitySessionId == session.Id) 
            {
                BytesReceived += e.Chunk.Length;
                OnDataReceived(e.Chunk);
            }
        }        

        void chatHost_ActivityInvitationAccepted(object? sender, ActivitySessionEventArgs e)
        {
            if (e.ActivitySessionId == session.Id)
            {
                _cts = new CancellationTokenSource();
                _progress = new Progress<int>(OnProgressChanged);
                var token = _cts.Token;

                Task.Run(() =>
                {
                    OnTransferStarted();
                    handler.TransferData(() => token.IsCancellationRequested);
                    return token.IsCancellationRequested;
                }, token).ContinueWith(task =>
                {
                    OnTransferFinished();

                    if (task.IsCanceled || (task.IsCompleted && !task.IsFaulted && task.Result))
                        OnTransferCancelled();
                    else if (task.IsFaulted)
                        OnError(task.Exception.InnerException);
                    else
                        OnTransferCompleted();
                });
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
