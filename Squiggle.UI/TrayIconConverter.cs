using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Squiggle.Chat;
using System.Windows.Media.Imaging;

namespace Squiggle.UI
{
    public class TrayIconConverter: IValueConverter
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
                        return new BitmapImage(new Uri("/Images/Chat.ico", UriKind.Relative));
                    case UserStatus.Busy:
                        return new BitmapImage(new Uri("/Images/Chat-Busy.ico", UriKind.Relative));
                    case UserStatus.BeRightBack:
                    case UserStatus.Away:
                    case UserStatus.Idle:
                        return new BitmapImage(new Uri("/Images/Chat-Away.ico", UriKind.Relative));
                    case UserStatus.Offline:
                        return new BitmapImage(new Uri("/Images/Chat-Offline.ico", UriKind.Relative));
                    default:
                        return new BitmapImage(new Uri("/Images/Chat.ico", UriKind.Relative));
                }
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
