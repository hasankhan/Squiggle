using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Squiggle.Chat;
using System.Windows;

namespace Squiggle.UI
{
    public class StatusConverter: IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var status = (UserStatus)value;
                switch (status)
                {
                    case UserStatus.OnPhone:
                        return "On Phone";
                    case UserStatus.InAMeeting:
                        return "In a Metting";
                    case UserStatus.OutForLunch:
                        return "Out for Lunch";
                    case UserStatus.BeRightBack:
                        return "Be Right Back";
                    default:
                        return status.ToString();
                }
            }
            else
                return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
