using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace Squiggle.UI.Controls.ChatItems
{
    class InfoItem:ChatItem
    {
        public string Info { get; private set; }

        public InfoItem(string info)
        {
            this.Info = info;
        }

        public override void AddTo(System.Windows.Documents.InlineCollection inlines)
        {
            string text = String.Format("[{0}] {1}", Stamp.ToShortTimeString(), Info);

            var run = new Run(text);
            run.Foreground = Brushes.DarkGray;
            inlines.Add(run);
        }
    }
}
