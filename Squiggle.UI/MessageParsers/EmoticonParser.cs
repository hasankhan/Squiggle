using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Squiggle.UI.Helpers;

namespace Squiggle.UI.MessageParsers
{
    class EmoticonParser: RegexParser
    {
        public static EmoticonParser Instance = new EmoticonParser();

        class EmoticonEntry
        {
            public BitmapImage Image { get; set; }
            public string Title { get; set; }
        }

        Dictionary<string, EmoticonEntry> emoticons = new Dictionary<string, EmoticonEntry>();
        Regex pattern;

        public EmoticonParser()
        {
            foreach (var emoticon in Emoticons.All)
                AddEmoticon(emoticon);
        }

        protected override Regex Pattern
        {
            get { return pattern; }
        }

        public void AddEmoticon(Emoticon emoticon)
        {
            var image = ImageFactory.Instance.Load(emoticon.ImageUri);

            var entry = new EmoticonEntry() { Image = image,
                                              Title = emoticon.Title };

            foreach (var code in emoticon.Codes)
                emoticons[code.ToUpperInvariant()] = entry;

            string regex = String.Join(")|(", emoticons.Keys.Select(c => Regex.Escape(c)).ToArray());
            pattern = new Regex("(" + regex + ")", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        protected override Inline Convert(string text)
        {
            EmoticonEntry emoticon = emoticons[text.ToUpperInvariant()];
            var image = new Image() { Source = emoticon.Image };
            image.Stretch = System.Windows.Media.Stretch.UniformToFill;
            image.Width = 24;
            image.Height = 24;
            return new InlineUIContainer(image);
        }
    }
}
