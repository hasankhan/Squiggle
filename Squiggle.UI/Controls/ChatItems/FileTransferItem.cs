using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Squiggle.Chat;
using Squiggle.Activity;
using Squiggle.Chat.Activities;

namespace Squiggle.UI.Controls.ChatItems
{
    class FileTransferItem: UIChatItem<FileTransferControl>
    {
        public bool Sending { get; private set; }
        public string DownloadsFolder { get; private set; }
        public IFileTransferHandler Session { get; private set; }

        public FileTransferItem(IFileTransferHandler session, string downloadsFolder)
        {
            this.Session = session;
            this.DownloadsFolder = downloadsFolder;
            this.Sending = false;
        }

        public FileTransferItem(IFileTransferHandler session)
        {
            this.Session = session;
            this.Sending = true;
        }

        protected override FileTransferControl CreateControl()
        {
            var control = new FileTransferControl(Session, Sending);
            if (!Sending)
                control.DownloadFolder = DownloadsFolder;
            return control;
        }
    }
}
