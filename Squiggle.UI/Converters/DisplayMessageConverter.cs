using System;
using System.Windows.Data;
using Squiggle.UI.Resources;

namespace Squiggle.UI.Converters
{
    class DisplayMessageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string message = value as string;
            if (String.IsNullOrEmpty(message))
                return Translation.MainWindow_ShareAMessage;

            return message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
