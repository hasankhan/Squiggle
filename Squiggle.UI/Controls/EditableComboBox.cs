using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Squiggle.UI;
using Squiggle.UI.Helpers;

namespace Squiggle.UI.Controls
{
    public class EditableComboBox
    {
        public static int GetMaxLength(DependencyObject obj)
        {
            return (int)obj.GetValue(MaxLengthProperty);
        }

        public static void SetMaxLength(DependencyObject obj, int value)
        {
            obj.SetValue(MaxLengthProperty, value);
        }

        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.RegisterAttached("MaxLength", typeof(int), typeof(EditableComboBox), new UIPropertyMetadata(OnMaxLenghtChanged));

        private static void OnMaxLenghtChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var comboBox = obj as ComboBox;
            if (comboBox == null) return;

            comboBox.Loaded +=
                (s, e) =>
                {
                    var textBox =  comboBox.GetVisualChildren<TextBox>(t=>t.Name == "PART_EditableTextBox").FirstOrDefault();
                    if (textBox == null) 
                        return;
                    textBox.SetValue(TextBox.MaxLengthProperty, args.NewValue);
                };
        }
    }

}
