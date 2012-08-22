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
            item.AddTo(para.Inlines);

            if (KeepHistory)
                history.Enqueue(item);

            para.Inlines.Add(new LineBreak());
            sentMessages.FindScrollViewer().ScrollToBottom();
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
            para.Inlines.Clear();
        }        
    }
}
