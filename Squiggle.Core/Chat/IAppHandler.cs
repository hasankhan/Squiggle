using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Squiggle.Core.Chat
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
