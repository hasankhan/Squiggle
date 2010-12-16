using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Squiggle.Chat;

namespace Squiggle.UI.Converters
{
    public class ContactListGroupConverter: IValueConverter
    {
        public static ContactListGroupConverter Instance = new ContactListGroupConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var buddies = ((IEnumerable<object>)value).OfType<Buddy>();
            return buddies.Where(b => b.Status != UserStatus.Offline).Count();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
