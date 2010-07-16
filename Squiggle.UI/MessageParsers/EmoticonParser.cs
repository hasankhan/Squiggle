using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Squiggle.UI.MessageParsers
{
    class EmoticonParser: IMessageParser
    {
        class EmoticonEntry
        {
            public BitmapImage Image { get; set; }
            public string Title { get; set; }
        }

        Dictionary<string, EmoticonEntry> emoticons = new Dictionary<string, EmoticonEntry>();
        Regex pattern;

        public Regex Pattern
        {
            get { return pattern; }
        }

        public EmoticonParser()
        {
            foreach (var emoticon in Emoticons.All)
                AddEmoticon(emoticon);
        }

        public void AddEmoticon(Squiggle.UI.Emoticon emoticon)
        {
            var entry = new EmoticonEntry() { Image = new BitmapImage(emoticon.ImageUri),
                                              Title = emoticon.Title };

            foreach (var code in emoticon.Codes)
                emoticons[code.ToLower()] = entry;

            string regex = String.Join(")|(", emoticons.Keys.Select(c => Regex.Escape(c)).ToArray());
            pattern = new Regex("(" + regex + ")", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public IEnumerable<Inline> ParseText(string text)
        {
            EmoticonEntry emoticon;
            if (emoticons.TryGetValue(text.ToLower(), out emoticon))
            {
                var image = new Image() { Source = emoticon.Image };
                image.Stretch = System.Windows.Media.Stretch.None;
                yield return new InlineUIContainer(image);
            }
        }
    }
}
