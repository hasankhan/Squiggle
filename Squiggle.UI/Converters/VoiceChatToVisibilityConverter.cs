using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Squiggle.UI.Converters
{
    public class VoiceChatToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null && parameter.ToString() == "voicechatoff")
                return Visibility.Visible;

            if (value != null && parameter.ToString() == "voicechaton")
                return Visibility.Visible;

            if (value == null && parameter.ToString() == "voicechaton")
                return Visibility.Hidden;

            if (value != null && parameter.ToString() == "voicechatoff")
                return Visibility.Hidden;

            return Visibility.Hidden;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
