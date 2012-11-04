using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Squiggle.Client;
using Squiggle.Core.Chat.Activity;
using Squiggle.Client.Activities;

namespace Squiggle.UI.Controls.ChatItems.Activity
{
    class FileTransferItem: ActivityChatItem<FileTransferControl, IFileTransferHandler>
    {
        public string DownloadsFolder { get; private set; }

        public FileTransferItem(IFileTransferHandler session, string downloadsFolder):base(session, sending:false)
        {
            this.DownloadsFolder = downloadsFolder;
        }

        public FileTransferItem(IFileTransferHandler session): base(session, sending: true) { }

        protected override FileTransferControl CreateControl()
        {
            var control = new FileTransferControl(Session, Sending);
            if (!Sending)
                control.DownloadFolder = DownloadsFolder;
            return control;
        }
    }
}
