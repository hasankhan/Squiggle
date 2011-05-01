using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Squiggle.Utilities;
using Squiggle.UI.Resources;

namespace Squiggle.UI.Converters
{
    public class EnumValueConverter: IValueConverter
    {
        public static EnumValueConverter Instance = new EnumValueConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string stringValue = StringValueAttribute.GetValue(value);
            string translatedValue = Translation.GetTranslation(stringValue);
            return translatedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
