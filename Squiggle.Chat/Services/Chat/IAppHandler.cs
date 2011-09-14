using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;

namespace Squiggle.Chat.Services.Chat
{
    public interface IAppHandler
    {
        event EventHandler TransferCompleted;
        event EventHandler TransferStarted;
        event EventHandler TransferCancelled;
        event EventHandler TransferFinished;
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        event EventHandler<ErrorEventArgs> Error;
        Guid AppId { get; }
        void Cancel();
    }
}
