using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Apps;

namespace Squiggle.Chat
{
    public class FileTransferCollection: System.Collections.Generic.SynchronizedCollection<IFileTransfer>
    {
        protected override void InsertItem(int index, IFileTransfer item)
        {
            base.InsertItem(index, item);
            item.TransferFinished += new EventHandler(item_TransferFinished);
        }

        protected override void RemoveItem(int index)
        {
            IFileTransfer item = this[index];
            base.RemoveItem(index);
            item.TransferFinished -= new EventHandler(item_TransferFinished);
        }

        void item_TransferFinished(object sender, EventArgs e)
        {
            Remove((IFileTransfer)sender);
        }

        public void CancelAll()
        {
            lock (SyncRoot)
                foreach (IFileTransfer item in this.ToList())
                    item.Cancel();
        }
    }
}
