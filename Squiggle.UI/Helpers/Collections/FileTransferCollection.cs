using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat.Activity;
using Squiggle.Client.Activities;

namespace Squiggle.UI.Helpers.Collections
{
    public class FileTransferCollection: Collection<IFileTransferHandler>
    {
        private readonly object _syncRoot = new object();

        protected object SyncRoot => _syncRoot;

        protected override void InsertItem(int index, IFileTransferHandler item)
        {
            lock (_syncRoot)
                base.InsertItem(index, item);
            item.TransferFinished += item_TransferFinished;
        }

        protected override void RemoveItem(int index)
        {
            IFileTransferHandler item;
            lock (_syncRoot)
            {
                item = this[index];
                base.RemoveItem(index);
            }
            item.TransferFinished -= item_TransferFinished;
        }

        void item_TransferFinished(object? sender, EventArgs e)
        {
            Remove((IFileTransferHandler)sender);
        }

        public void CancelAll()
        {
            lock (_syncRoot)
                foreach (IFileTransferHandler item in this.ToList())
                    item.Cancel();
        }
    }
}
