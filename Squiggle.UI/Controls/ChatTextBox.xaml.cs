using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChatTextBox.xaml
    /// </summary>
    public partial class ChatTextBox : UserControl
    {
        public ChatTextBox()
        {
            InitializeComponent();

            sentMessages.Document = new FlowDocument();
            sentMessages.Document.Blocks.Add(new Paragraph());
        }

        public void AddError(string message, string detail)
        {
            var para = sentMessages.Document.Blocks.FirstBlock as Paragraph;

            var text = new Run(message);
            text.Foreground = new SolidColorBrush(Colors.Red);

            para.Inlines.Add(text);
            para.Inlines.Add(new Run("\r\n\t"));
            para.Inlines.Add(detail);
            para.Inlines.Add(new LineBreak());

            scrollViewer.ScrollToBottom();
        }

        public void AddMessage(string user, string message)
        {
            var title = new Bold(new Run(user + ": "));
            var text = new Run(message);
            var para = sentMessages.Document.Blocks.FirstBlock as Paragraph;

            para.Inlines.Add(title);
            para.Inlines.Add(text);
            para.Inlines.Add(new LineBreak());
            scrollViewer.ScrollToBottom();
        }
    }
}
