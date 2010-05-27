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

        public void AddError(string error, string detail)
        {
            var para = sentMessages.Document.Blocks.FirstBlock as Paragraph;

            var errorText = new Run(error);
            errorText.Foreground = new SolidColorBrush(Colors.Red);

            var detailText = new Run(detail);
            detailText.Foreground = new SolidColorBrush(Colors.Gray);

            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(errorText);
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(detailText);
            para.Inlines.Add(new LineBreak());
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
