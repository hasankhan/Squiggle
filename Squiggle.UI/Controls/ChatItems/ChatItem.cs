using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace Squiggle.UI.Controls.ChatItems
{
    public abstract class ChatItem
    {
        public DateTime Stamp { get; private set; }

        public ChatItem()
        {
            Stamp = DateTime.Now;
        }

        public abstract void AddTo(InlineCollection inlines);
        public virtual void Remove() { }
    }
}
