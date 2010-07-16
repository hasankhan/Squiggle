using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI
{
    static class Emoticons
    {
        public static readonly Emoticon Angry = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/angry.png"), "Angry", ":@", ":-@");
        public static readonly Emoticon Crying = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/crying.png"), "Crying", ":'(");
        public static readonly Emoticon Embarrassed = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/embarrassed.png"), "Embarrassed", ":-$", ":$");
        public static readonly Emoticon Hot = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/hot.png"), "Hot", "(h)");
        public static readonly Emoticon OpenMouthed = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/laughing.png"), "Open-mouthed", ":-D", ":d");
        public static readonly Emoticon Nerd = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/nerd.png"), "Nerd", "8-|");
        public static readonly Emoticon Sad = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/sad.png"), "Sad", ":-(", ":(");
        public static readonly Emoticon Smile = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/smile.png"), "Smile", ":-)", ":)");
        public static readonly Emoticon Surprised = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/surprize.png"), "Surprised", ":-O", ":o");
        public static readonly Emoticon TongueOut = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/tounge.png"), "Tongue out", ":-p", ":p");
        public static readonly Emoticon Wink = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/wink.png"), "Wink", ";-)", ";)");

        static List<Emoticon> allEmoticons = new List<Emoticon>();
        public static IEnumerable<Emoticon> All
        {
            get { return allEmoticons; }
        }

        static Emoticons()
        {
            allEmoticons.AddRange(from emoticon in typeof(Emoticons).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                                  where emoticon.FieldType.Equals(typeof(Emoticon))
                                  select (Emoticon)emoticon.GetValue(null));
        }
    }
}
