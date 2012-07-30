using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Squiggle.Utilities;
using Squiggle.Core.Chat;

namespace Squiggle.Activities.FileTransfer
{
    class FileTransfer: AppHandler, IFileTransfer
    {
        Stream content;
        string filePath;

        public long Size { get; private set; }
        public string Name { get; private set; }

        public override Guid AppId
        {
            get { return SquiggleActivities.FileTransfer; }
        }

        public FileTransfer(AppSession session, string name, long size, Stream content)
            :base(session)
        {
            this.Name = name;
            this.Size = size;
            this.content = content;
        }

        public FileTransfer(AppSession session, string name, long size)
            :base(session)
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
            content = File.OpenWrite(filePath);

            base.OnAccept();
        }

        protected override void OnTransferCancelled()
        {
            if (!SelfInitiated && content != null)
                File.Delete(filePath);

            base.OnTransferCancelled();
        }

        protected override void OnTransferFinished()
        {
            if (content != null)
            {
                content.Dispose();
                content = null;
            }

            base.OnTransferFinished();
        }

        protected override void OnDataReceived(byte[] chunk)
        {
            if (!SelfInitiated && content != null)
            {
                content.Write(chunk, 0, chunk.Length);

                float progress = BytesReceived / (float)Size * 100;
                OnProgressChanged((int)progress);

                if (BytesReceived >= Size)
                    CompleteTransfer();
            }
        }

        protected override void TransferData(Func<bool> cancelPending)
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
