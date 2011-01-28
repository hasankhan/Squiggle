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

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ChatTextBox.xaml
    /// </summary>
    public partial class ChatTextBox : UserControl
    {
        MultiParser parsers;
        public IList<IMessageParser> MessageParsers
        {
            get { return parsers; }
        }

        public ChatTextBox()
        {
            InitializeComponent();
            
            parsers = new MultiParser();
            MessageParsers.Add(HyperlinkParser.Instance);
        }        

        public void AddInfo(string info)
        {
            info = String.Format("[{0}] {1}", DateTime.Now.ToShortTimeString(), info);

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

        public void AddMessage(string user, string message, string fontName, int fontSize, System.Drawing.FontStyle fontStyle, System.Drawing.Color color)
        {
            var para = sentMessages.Document.Blocks.FirstBlock as Paragraph;

            string text = String.Format("{0} " + Translation.Instance.Global_ContactSaid + " ({1}): ", user, DateTime.Now.ToShortTimeString());
            var items = parsers.ParseText(text);
            foreach (var item in items)
            {
                item.Foreground = Brushes.Gray;
                para.Inlines.Add(item);
            }
            para.Inlines.Add(new LineBreak());
            items = parsers.ParseText(message);
            var fontsettings = new FontSetting(color, fontName, fontSize, fontStyle);
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

        public void Clear()
        {
            sentMessages.Document.Blocks.Clear();
        }
    }
}
