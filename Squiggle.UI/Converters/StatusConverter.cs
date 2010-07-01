using System;
using System.Windows;
using System.Windows.Data;
using Squiggle.Chat;

namespace Squiggle.UI.Converters
{
    public class StatusConverter: IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is UserStatus)
            {
                var status = (UserStatus)value;
                switch (status)
                {
                    case UserStatus.Online:
                        return "Online";
                    case UserStatus.Busy:
                        return "Busy";                  
                    case UserStatus.BeRightBack:
                        return "Be Right Back";
                    case UserStatus.Away:
                        return "Away";
                    case UserStatus.Idle:
                        return "Idle";
                    case UserStatus.Offline:
                        return "Offline";
                    default:
                        return status.ToString();
                }
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
