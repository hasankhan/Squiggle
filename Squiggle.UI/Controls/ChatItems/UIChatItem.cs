using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;

namespace Squiggle.UI.Controls.ChatItems
{
    abstract class UIChatItem<T>: ChatItem where T:Control
    {
        T control;

        public override void AddTo(InlineCollection inlines)
        {
            if (control == null)
                control = CreateControl();
            inlines.Add(new InlineUIContainer(control));
        }

        public override void Remove()
        {
            ((InlineUIContainer)control.Parent).Child = null;

            base.Remove();
        }

        protected abstract T CreateControl();
    }
}
