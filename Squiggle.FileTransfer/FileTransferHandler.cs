using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Squiggle.Client.Activities;
using Squiggle.Core.Chat.Activity;

namespace Squiggle.FileTransfer
{
    /// <summary>
    /// Handles peer-to-peer file transfer over the gRPC activity stream.
    /// The sender reads the file in 32KB chunks and pushes them via <see cref="ActivityHandler.SendData"/>,
    /// which routes through ActivityExecutor → ActivitySession → ChatHost → gRPC.
    /// The receiver writes incoming chunks to disk and signals completion when all bytes arrive.
    /// </summary>
    class FileTransferHandler : ActivityHandler, IFileTransferHandler
    {
        Stream? content;
        string? filePath;

        /// <inheritdoc />
        public long Size { get; private set; }

        /// <inheritdoc />
        public string Name { get; private set; } = null!;

        /// <summary>
        /// Initializes a sender-side handler with file content to transfer.
        /// </summary>
        public FileTransferHandler(IActivityExecutor executor, string name, long size, Stream content)
            : base(executor)
        {
            Name = name;
            Size = size;
            this.content = content;
        }

        /// <summary>
        /// Initializes a receiver-side handler (content stream opened on accept).
        /// </summary>
        public FileTransferHandler(IActivityExecutor executor, string name, long size)
            : base(executor)
        {
            Name = name;
            Size = size;
        }

        /// <inheritdoc />
        public override IDictionary<string, string> CreateInviteMetadata()
        {
            return new FileInviteData { Name = Name, Size = Size }
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Accepts the incoming file transfer, saving to <paramref name="fileName"/>.
        /// </summary>
        public void Accept(string fileName)
        {
            filePath = fileName;
            Start();
        }

        /// <inheritdoc />
        public override void OnAccept()
        {
            content = File.OpenWrite(filePath!);
        }

        /// <inheritdoc />
        public override void OnTransferCancelled()
        {
            if (!SelfInitiated && filePath != null)
            {
                try { File.Delete(filePath); }
                catch (IOException) { /* best effort cleanup */ }
            }

            base.OnTransferCancelled();
        }

        /// <inheritdoc />
        public override void OnTransferFinished()
        {
            if (content != null)
            {
                content.Dispose();
                content = null;
            }

            base.OnTransferFinished();
        }

        /// <inheritdoc />
        public override void OnDataReceived(byte[] chunk)
        {
            if (!SelfInitiated && content != null)
            {
                content.Write(chunk, 0, chunk.Length);

                int progress = (int)(BytesReceived / (float)Size * 100);
                OnProgressChanged(progress);

                if (BytesReceived >= Size)
                    CompleteTransfer();
            }
        }

        /// <summary>
        /// Reads the source stream in chunks and sends each chunk to the peer via gRPC activity data.
        /// Called on the sender side after the receiver accepts the invite.
        /// </summary>
        public override void TransferData(Func<bool> cancelPending)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            long bytesRemaining = Size;

            while (bytesRemaining > 0 && !cancelPending())
            {
                int bytesRead = content!.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                byte[] chunk = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, chunk, 0, bytesRead);
                SendData(chunk);

                bytesRemaining -= bytesRead;
                int progress = (int)((Size - bytesRemaining) / (float)Size * 100);
                UpdateProgress(progress);
            }
        }
    }
}
