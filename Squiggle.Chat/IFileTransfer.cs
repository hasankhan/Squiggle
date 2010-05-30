using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace Squiggle.Chat
{
    public class ChunkReceivedEventArgs: EventArgs
    {
        public byte[] Chunk {get; set; }
    }

    public interface IFileTransfer
    {
        event EventHandler TransferCompleted;
        event EventHandler TransferStarted;
        event EventHandler TransferCancelled;
        event EventHandler<ChunkReceivedEventArgs> ChunkReceived;
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        event EventHandler<ErrorEventArgs> Error;

        void Cancel();
        void Accept(string filePath);
    }
}
