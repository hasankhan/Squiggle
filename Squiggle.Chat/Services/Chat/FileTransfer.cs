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
    class FileTransfer: AppHandler, IFileTransfer
    {
        public event EventHandler TransferCompleted = delegate { };
        public event EventHandler TransferStarted = delegate { };
        public event EventHandler TransferCancelled = delegate { };
        public event EventHandler TransferFinished = delegate { };
        public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> ProgressChanged = delegate { };

        Stream content;
        string saveToFile;

        public long Size { get; private set; }
        public string Name { get; private set; }

        public override Guid AppId
        {
            get { return ChatApps.FileTransfer; }
        }

        public string filePath {get; set; }

        public FileTransfer(Guid sessionId, IChatHost remoteHost, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, string name, long size, Stream content)
            :base(sessionId, remoteHost, localHost, localUser, remoteUser)
        {
            this.Name = name;
            this.Size = size;
            this.content = content;
        }

        public FileTransfer(Guid sessionId, IChatHost remoteHost, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, string name, long size, Guid appSessionId)
            :base(sessionId, remoteHost, localHost, localUser, remoteUser, appSessionId)
        {
            this.Name = name;
            this.Size = size;
        }

        protected override IEnumerable<KeyValuePair<string, string>> CreateInviteMetadata()
        {
            IEnumerable<KeyValuePair<string, string>> data = new FileInviteData() { Name = Name, Size = Size };
            return data;
        }

        public void Accept(string fileName)
        {
            filePath = fileName;
            this.Accept();
        }

        protected override void OnAccept()
        {
            base.OnAccept();

            saveToFile = filePath;
            content = File.OpenWrite(filePath);
        }

        protected override void OnTransferCompleted()
        {
            base.OnTransferCompleted();

            TransferCompleted(this, EventArgs.Empty);
        }

        protected override void OnTransferCancelled()
        {
            base.OnTransferCancelled();

            if (!Sending && content != null)
                File.Delete(saveToFile);
            
            TransferCancelled(this, EventArgs.Empty);
        }

        protected override void OnTransferStarted()
        {
            base.OnTransferStarted();

            TransferStarted(this, EventArgs.Empty);
        }

        protected override void OnProgressChanged(int percentage)
        {
            base.OnProgressChanged(percentage);

            ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
        }

        protected override void OnTransferFinished()
        {
            base.OnTransferFinished();

            if (content != null)
            {
                content.Dispose();
                content = null;
            }
            TransferFinished(this, EventArgs.Empty);
        }

        protected override void OnDataReceived(byte[] chunk)
        {
            if (content != null)
            {
                content.Write(chunk, 0, chunk.Length);

                float progress = BytesReceived / (float)Size * 100;
                OnProgressChanged((int)progress);

                if (BytesReceived >= Size)
                    CompleteTransfer();
            }
        }

        protected override void OnSendData(Func<bool> cancelPending)
        {
            byte[] buffer = new byte[bufferSize];
            long bytesRemaining = Size;
            while (bytesRemaining > 0 && !cancelPending())
            {
                int bytesRead = content.Read(buffer, 0, buffer.Length);
                byte[] temp = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, temp, 0, temp.Length);
                SendData(temp);
                bytesRemaining -= bytesRead;
                float progress = (Size - bytesRemaining) / (float)Size * 100;
                UpdateProgress((int)progress);
            }
        }
    }
}
