using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Resources;

namespace Squiggle.UI
{
    static class Emoticons
    {
        public static readonly Emoticon Angry = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/angry.png"), 
                                                             Translation.Instance.Emoticon_Angry, 
                                                             ":@", ":-@");

        public static readonly Emoticon Crying = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/crying.png"), 
                                                              Translation.Instance.Emoticon_Crying, 
                                                              ":'(");

        public static readonly Emoticon Disappointed = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/disappointed.png"), 
                                                                    Translation.Instance.Emoticon_Disappointed, 
                                                                    ":-|", ":|");

        public static readonly Emoticon Hot = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/hot.png"),
                                                            Translation.Instance.Emoticon_Hot, 
                                                            "(h)");

        public static readonly Emoticon OpenMouthed = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/laughing.png"), 
                                                                    Translation.Instance.Emoticon_Laughing, 
                                                                    ":-D", ":d");

        public static readonly Emoticon Sad = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/sad.png"), 
                                                            Translation.Instance.Emoticon_Sad, 
                                                            ":-(", ":(");

        public static readonly Emoticon Sarcastic = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/sarcasm.png"), 
                                                                Translation.Instance.Emoticon_Sarcastic, 
                                                                "^o)");

        public static readonly Emoticon Silence = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/silence.png"), 
                                                                Translation.Instance.Emoticon_Silence, 
                                                                ":-#");

        public static readonly Emoticon Smile = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/smile.png"), 
                                                             Translation.Instance.Emoticon_Smile, 
                                                             ":-)", ":)");

        public static readonly Emoticon Surprised = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/surprize.png"), 
                                                                Translation.Instance.Emoticon_Surprised, 
                                                                ":-O", ":o");

        public static readonly Emoticon TongueOut = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/tounge.png"), 
                                                                Translation.Instance.Emoticon_ToungeOut, 
                                                                ":-p", ":p");

        public static readonly Emoticon Wink = new Emoticon(new Uri("pack://application:,,,/Images/Emoticons/wink.png"), 
                                                            Translation.Instance.Emoticon_Wink, 
                                                            ";-)", ";)");

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
