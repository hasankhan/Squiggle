using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace Squiggle.UI.Controls.ChatItems
{
    class ErrorItem: ChatItem
    {
        public string Error { get; private set; }
        public string Detail { get; private set; }

        public ErrorItem(string error, string detail)
        {
            this.Error = error;
            this.Detail = detail;
        }

        public override void AddTo(System.Windows.Documents.InlineCollection inlines)
        {
            var errorText = new Run(Error);
            errorText.Foreground = Brushes.Red;            
            inlines.Add(errorText);

            if (!String.IsNullOrEmpty(Detail))
            {
                var detailText = new Run(Detail);
                detailText.Foreground = Brushes.Gray;

                inlines.Add(new LineBreak());
                inlines.Add(detailText);
            } 
        }
    }
}
