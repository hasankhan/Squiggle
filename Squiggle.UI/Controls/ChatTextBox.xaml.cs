using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using Squiggle.Chat;
using Squiggle.UI.MessageParsers;
using Squiggle.UI.MessageFilters;
using Squiggle.UI.Resources;
using Squiggle.UI.Controls.ChatItems;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChatTextBox.xaml
    /// </summary>
    public partial class ChatTextBox : UserControl
    {
        MultiParser parsers;
        public MultiParser MessageParsers
        {
            get { return parsers; }
        }

        public ChatTextBox()
        {
            InitializeComponent();
            
            parsers = new MultiParser();
            MessageParsers.Add(HyperlinkParser.Instance);
        }        

        public void AddItem(ChatItem item)
        {
            item.AddTo(para.Inlines);
            para.Inlines.Add(new LineBreak());
            sentMessages.FindScrollViewer().ScrollToBottom();
        }

        public void SaveTo(string fileName)
        {
            var range = new TextRange(sentMessages.Document.ContentStart, sentMessages.Document.ContentEnd);
            using (var stream = new FileStream(fileName, FileMode.OpenOrCreate))
                range.Save(stream, DataFormats.Rtf);
        }

        public void Clear()
        {
            para.Inlines.Clear();
        }        
    }
}
