using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Squiggle.Client;
using Squiggle.UI.Controls.ChatItems;
using Squiggle.UI.Helpers;
using Squiggle.UI.MessageFilters;
using Squiggle.UI.MessageParsers;
using Squiggle.UI.Resources;
using Squiggle.UI.Helpers.Collections;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChatTextBox.xaml
    /// </summary>
    public partial class ChatTextBox : UserControl
    {
        BoundedQueue<ChatItem> history = new BoundedQueue<ChatItem>(100);

        public bool KeepHistory { get; set; }

        public ChatTextBox()
        {
            InitializeComponent();                      
        }        

        public void AddItem(ChatItem item)
        {
            var span = new Span();
            span.Tag = item;
            item.AddTo(span.Inlines);

            if (KeepHistory)
                history.Enqueue(item);

            Root.Inlines.Add(span);
            Root.Inlines.Add(new LineBreak());
            sentMessages.FindScrollViewer().ScrollToBottom();
        }

        public void UpdateItem<TITem>(Predicate<TITem> criteria, Action<TITem> updateAction) where TITem:ChatItem
        {
            Span result = Root.Inlines.OfType<Span>().Where(span => span.Tag is TITem && criteria((TITem)span.Tag)).FirstOrDefault();
            if (result != null)
            {
                var item = (TITem)result.Tag;
                updateAction(item);
                result.Inlines.Clear();
                item.AddTo(result.Inlines);
            }
        }

        public IEnumerable<ChatItem> GetHistory()
        {
            return history.ToList();
        }

        public void SaveTo(string fileName)
        {
            var range = new TextRange(sentMessages.Document.ContentStart, sentMessages.Document.ContentEnd);
            using (var stream = new FileStream(fileName, FileMode.OpenOrCreate))
                range.Save(stream, DataFormats.Rtf);
        }

        public void Clear()
        {
            history.Clear();
            Root.Inlines.Clear();
        }        
    }
}
