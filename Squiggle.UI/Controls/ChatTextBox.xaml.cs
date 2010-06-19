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
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Squiggle.Chat;
using System.Windows.Markup;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChatTextBox.xaml
    /// </summary>
    public partial class ChatTextBox : UserControl
    {
        static Regex urlRegex = new Regex(@"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9=:\-\.\?\,\'\/\\\+&%\$#_]*)?");

        public ChatTextBox()
        {
            InitializeComponent();
        }

        public void AddInfo(string info)
        {
            var errorText = new Run(info);
            errorText.Foreground = new SolidColorBrush(Colors.DarkGray);
            para.Inlines.Add(errorText);
            para.Inlines.Add(new LineBreak());

            sentMessages.FindScrollViewer().ScrollToBottom();
        }

        public void AddError(string error, string detail)
        {
            var errorText = new Run(error);
            errorText.Foreground = new SolidColorBrush(Colors.Red);

            var detailText = new Run(detail);
            detailText.Foreground = new SolidColorBrush(Colors.Gray);

            para.Inlines.Add(errorText);
            if (!String.IsNullOrEmpty(detail))
            {
                para.Inlines.Add(new LineBreak());
                para.Inlines.Add(detailText);
            }            
            para.Inlines.Add(new LineBreak());

            sentMessages.FindScrollViewer().ScrollToBottom();
        }

        public void AddMessage(string user, string message, string fontName, int fontSize, string fontStyle, bool bold, System.Drawing.Color color)
        {
            var para = sentMessages.Document.Blocks.FirstBlock as Paragraph;

            string text = String.Format("{0} said ({1}): ", user, DateTime.Now.ToShortTimeString());
            var items = ParseText(text);
            foreach (var item in items)
            {
                item.Foreground = Brushes.Gray;
                para.Inlines.Add(item);
            }
            para.Inlines.Add(new LineBreak());
            items = ParseText(message);
            var fontsettings = new FontSetting(color, fontName, fontSize, fontStyle, bold);
            foreach (var item in items)
            {
                item.FontFamily = fontsettings.Family;
                item.FontSize = fontsettings.Size;
                item.Foreground = fontsettings.Foreground;
                item.FontStyle = fontsettings.Style;
                item.FontWeight = fontsettings.Weight;
                para.Inlines.Add(item);
            }

            para.Inlines.AddRange(items);
            para.Inlines.Add(new LineBreak());
            sentMessages.FindScrollViewer().ScrollToBottom();
        }

        public void AddFileReceiveRequest(string user, IFileTransfer fileTransfer, string downloadsFolder)
        {
            var para = sentMessages.Document.Blocks.FirstBlock as Paragraph;
            var transferUI = new FileTarnsferControl(fileTransfer, false);
            transferUI.DownloadFolder = downloadsFolder;
            para.Inlines.Add(new InlineUIContainer(transferUI));
            para.Inlines.Add(new LineBreak());
        }

        public void AddFileSentRequest(IFileTransfer fileTransfer)
        {
            var para = sentMessages.Document.Blocks.FirstBlock as Paragraph;
            var transferUI = new FileTarnsferControl(fileTransfer, true);
            para.Inlines.Add(new InlineUIContainer(transferUI));
            para.Inlines.Add(new LineBreak());
        }

        public void SaveTo(string fileName)
        {
            var range = new TextRange(sentMessages.Document.ContentStart, sentMessages.Document.ContentEnd);
            using (var stream = new FileStream(fileName, FileMode.OpenOrCreate))
                range.Save(stream, DataFormats.Rtf);
        }

        static List<Inline> ParseText(string message)
        {
            var items = new List<Inline>();
            int lastIndex = 0;
            foreach (Match match in urlRegex.Matches(message))
            {
                string text = message.Substring(lastIndex, match.Index - lastIndex);
                AddText(items, text);
                AddHyperlink(items, match.Value);
                lastIndex = match.Index + match.Length;
            }
            AddText(items, message.Substring(lastIndex));
            return items;
        }

        static void AddText(List<Inline> items, string text)
        {
            if (!String.IsNullOrEmpty(text))
                items.Add(new Run(text));
        }

        static void AddHyperlink(List<Inline> items, string url)
        {
            var link = new Hyperlink(new Run(url));
            link.NavigateUri = new Uri(url, UriKind.Absolute);
            link.Cursor = Cursors.Hand;
            link.RequestNavigate += (s, e) =>
            {
                OpenLink(link);
                e.Handled = true;
            };
            items.Add(link);
        }

        static void OpenLink(Hyperlink link)
        {
            Shell.OpenUrl(link.NavigateUri.AbsoluteUri);
        }

    }
    
}
