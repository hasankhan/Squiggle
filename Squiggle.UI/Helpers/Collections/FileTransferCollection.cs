using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Activities;

namespace Squiggle.UI.Helpers.Collections
{
    public class FileTransferCollection: System.Collections.Generic.SynchronizedCollection<IFileTransferHandler>
    {
        protected override void InsertItem(int index, IFileTransferHandler item)
        {
            base.InsertItem(index, item);
            item.TransferFinished += new EventHandler(item_TransferFinished);
        }

        protected override void RemoveItem(int index)
        {
            IFileTransferHandler item = this[index];
            base.RemoveItem(index);
            item.TransferFinished -= new EventHandler(item_TransferFinished);
        }

        void item_TransferFinished(object sender, EventArgs e)
        {
            Remove((IFileTransferHandler)sender);
        }

        public void CancelAll()
        {
            lock (SyncRoot)
                foreach (IFileTransferHandler item in this.ToList())
                    item.Cancel();
        }
    }
}
