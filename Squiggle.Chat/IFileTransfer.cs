using System;
using System.ComponentModel;
using System.IO;

namespace Squiggle.Chat
{
    public interface IFileTransfer
    {
        event EventHandler TransferCompleted;
        event EventHandler TransferStarted;
        event EventHandler TransferCancelled;
        event EventHandler TransferFinished;
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        event EventHandler<ErrorEventArgs> Error;

        int Size { get; }
        string Name { get; }

        void Cancel();
        void Accept(string filePath);
    }
}
