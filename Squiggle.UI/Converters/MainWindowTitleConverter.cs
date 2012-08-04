using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Squiggle.UI.ViewModel;

namespace Squiggle.UI.Converters
{
    class MainWindowTitleConverter : IValueConverter
    {
        public static MainWindowTitleConverter Instance = new MainWindowTitleConverter();
 
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var client = value as ClientViewModel;
            if (client == null)
                return null;

            if (client.IsLoggedIn)
                return String.Format("Squiggle Messenger - {0}", client.LoggedInUser.DisplayName);
            return "Squiggle Messenger";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
