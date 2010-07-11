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
        class Emoticon
        {
            public BitmapImage Image { get; set; }
            public string Title { get; set; }
        }

        Dictionary<string, Emoticon> emoticons = new Dictionary<string, Emoticon>();
        Regex pattern;

        public Regex Pattern
        {
            get { return pattern; }
        }

        public EmoticonParser()
        {
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/angry.png"), "Angry", ":@", ":-@");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/crying.png"), "Crying", ":'(");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/embarrassed.png"), "Embarrassed", ":-$", ":$");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/hot.png"), "Hot", "(h)");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/laughing.png"), "Open-mouthed", ":-D", ":d");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/nerd.png"), "Nerd", "8-|");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/sad.png"), "Sad", ":-(", ":(");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/smile.png"), "Smile", ":-)", ":)");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/surprize.png"), "Surprised", ":-O", ":o");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/tounge.png"), "Tongue out", ":-p", ":p");
            AddEmoticon(new Uri("pack://application:,,,/Images/Emoticons/wink.png"), "Wink", ";-)", ";)");
        }

        public void AddEmoticon(Uri imageUrl, string title, params string[] codes)
        {
            var emoticon = new Emoticon() { Image = new BitmapImage(imageUrl),
                                            Title = title };

            foreach (var code in codes)
                emoticons[code.ToLower()] = emoticon;

            string regex = String.Join(")|(", emoticons.Keys.Select(c => Regex.Escape(c)).ToArray());
            pattern = new Regex("(" + regex + ")", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public IEnumerable<Inline> ParseText(string text)
        {
            Emoticon emoticon;
            if (emoticons.TryGetValue(text.ToLower(), out emoticon))
            {
                var image = new Image() { Source = emoticon.Image };
                image.Stretch = System.Windows.Media.Stretch.None;
                yield return new InlineUIContainer(image);
            }
        }
    }
}
