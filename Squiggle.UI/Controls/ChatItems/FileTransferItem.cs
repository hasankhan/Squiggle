using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;
using System.Windows.Documents;

namespace Squiggle.UI.Controls.ChatItems
{
    class FileTransferItem: UIChatItem<FileTransferControl>
    {
        public bool Sending { get; private set; }
        public string DownloadsFolder { get; private set; }
        public IFileTransfer Session { get; private set; }

        public FileTransferItem(IFileTransfer session, string downloadsFolder)
        {
            this.Session = session;
            this.DownloadsFolder = downloadsFolder;
            this.Sending = false;
        }

        public FileTransferItem(IFileTransfer session)
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
