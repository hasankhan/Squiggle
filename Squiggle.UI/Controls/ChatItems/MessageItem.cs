using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using Squiggle.UI.MessageParsers;
using Squiggle.UI.Resources;
using System.Windows.Controls;
using Squiggle.UI.Helpers;

namespace Squiggle.UI.Controls.ChatItems
{
    class MessageItem: ChatItem
    {
        public Guid Id { get; private set; }
        public string User { get; private set; }
        public string Message { get; private set; }
        public string FontName { get; private set; }
        public int FontSize { get; private set; }
        public System.Drawing.FontStyle FontStyle { get; private set; }
        public System.Drawing.Color Color { get; private set; }
        public MultiParser Parsers { get; private set; }
        public bool Updated { get; private set; }

        public MessageItem(string user, Guid id, string message, string fontName, int fontSize, System.Drawing.FontStyle fontStyle, System.Drawing.Color color, MultiParser parsers)
        {
            this.User = user;
            this.Id = id;
            this.Message = message;
            this.FontName = fontName;
            this.FontSize = fontSize;
            this.FontStyle = fontStyle;
            this.Color = color;
            this.Parsers = parsers;
        }

        public override void AddTo(InlineCollection inlines)
        {
            if (Updated)
                AddUpdatedImage(inlines);

            AddContactSays(inlines);
            inlines.Add(new LineBreak());
            AddMessage(inlines);
        }

        public void Update(string message)
        {
            Message = message;
            Updated = true;
        }

        void AddUpdatedImage(InlineCollection inlines)
        {
            var image = new Image() 
            { 
                Margin = new System.Windows.Thickness(0),
                Width = 10,
                Height = 10
            };
            image.Source = ImageFactory.Instance.Load(new Uri("pack://application:,,,/Images/msg-edit.png", UriKind.Absolute));
            
            var container = new BlockUIContainer(image) 
            { 
                Margin = new System.Windows.Thickness(0),
                Padding = new System.Windows.Thickness(0)
            };

            var floater = new Floater(container)
            {
                Width = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                BaselineAlignment = System.Windows.BaselineAlignment.Top,
                Margin = new System.Windows.Thickness(0),
                Padding = new System.Windows.Thickness(0)
            };
            inlines.Add(floater);
        }

        void AddContactSays(InlineCollection inlines)
        {
            string text = String.Format("{0} " + Translation.Instance.Global_ContactSaid + " ({1}): ", this.User, Stamp.ToShortTimeString());
            var items = Parsers.ParseText(text);
            foreach (var item in items)
                item.Foreground = Brushes.Gray;
            inlines.AddRange(items);
        }

        void AddMessage(InlineCollection inlines)
        {
            var items = Parsers.ParseText(Message);
            var fontsettings = new FontSetting(Color, FontName, FontSize, FontStyle);

            foreach (var item in items)
            {
                item.FontFamily = fontsettings.Family;
                item.FontSize = fontsettings.Size;
                item.Foreground = fontsettings.Foreground;
                item.FontStyle = fontsettings.Style;
                item.FontWeight = fontsettings.Weight;
            }

            inlines.AddRange(items);
        }
    }
}
