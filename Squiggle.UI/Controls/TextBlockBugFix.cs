using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.ComponentModel;

namespace Squiggle.UI.Controls
{
    public class TextBlockBugFix
    {
        public static readonly DependencyProperty Text2Property = DependencyProperty.RegisterAttached("Text2", typeof(string), typeof(TextBlockBugFix), new FrameworkPropertyMetadata(OnTextPropertyChanged));

        public static void SetText2(UIElement element, string value)
        {           
            element.SetValue(Text2Property, value);
        }

        public static string GetText2(UIElement element)
        {
            return (string)element.GetValue(Text2Property);
        }

        static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = (TextBlock)sender;
            textBlock.Inlines.Clear();
            //http://stackoverflow.com/questions/4835920/round-brackets-not-showing-up-correctly-in-rightoleft-flow-direction-in-wpf/4837069#4837069
            var bugFixRun = new Run("i") { FontSize = .01 };
            var textRun = new Run(e.NewValue as string);
            textBlock.Inlines.Add(textRun);
            textBlock.Inlines.Add(bugFixRun);
        }
    }
}
