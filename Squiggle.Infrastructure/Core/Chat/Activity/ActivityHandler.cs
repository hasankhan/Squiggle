using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Squiggle.Core.Chat.Activity
{
    public abstract class ActivityHandler: IActivityHandler
    {
        protected const int BUFFER_SIZE = 32768; // 32KB

        IActivityExecutor executor;

        public event EventHandler<ErrorEventArgs> Error = delegate { };
        public event EventHandler TransferCompleted = delegate { };
        public event EventHandler TransferStarted = delegate { };
        public event EventHandler TransferCancelled = delegate { };
        public event EventHandler TransferFinished = delegate { };
        public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> ProgressChanged = delegate { };

        public ActivityHandler(IActivityExecutor executor)
        {
            this.executor = executor;
            executor.SetHandler(this);
        }

        protected bool SelfCancelled 
        {
            get { return executor.SelfCancelled; } 
        }
        
        protected bool IsConnected
        {
            get { return executor.IsConnected; }
        }
        
        protected bool SelfInitiated
        {
            get { return executor.SelfInitiated; } 
        }

        protected long BytesReceived
        {
            get { return executor.BytesReceived; }
        }

        public void Start()
        {
            if (SelfInitiated)
                executor.Start();
            else
                executor.Accept();
        }

        public void Cancel()
        {
            executor.Cancel();
        }

        public abstract void TransferData(Func<bool> cancelPending);

        public abstract IDictionary<string, string> CreateInviteMetadata();

        protected void SendData(byte[] chunk)
        {
            executor.SendData(chunk);
        }

        protected void UpdateProgress(int percentage)
        {
            executor.UpdateProgress(percentage);
        }

        protected void CompleteTransfer()
        {
            executor.CompleteTransfer();
        }

        public abstract void OnDataReceived(byte[] chunk);

        public virtual void OnAccept(){}

        public virtual void OnError(Exception ex)
        {
            Error(this, new ErrorEventArgs(ex));
        }

        public virtual void OnTransferCompleted()
        {
            TransferCompleted(this, EventArgs.Empty);
        }

        public virtual void OnTransferStarted()
        {
            TransferStarted(this, EventArgs.Empty);
        }

        public virtual void OnTransferCancelled()
        {
            TransferCancelled(this, EventArgs.Empty);
        }

        public virtual void OnTransferFinished()
        {
            TransferFinished(this, EventArgs.Empty); ;
        }

        public virtual void OnProgressChanged(int percentage)
        {
            ProgressChanged(this, new System.ComponentModel.ProgressChangedEventArgs(percentage, null));
        }        
    }
}
