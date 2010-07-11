using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Squiggle.Chat;

namespace Squiggle.UI.Converters
{
    public class TrayIconConverter: IValueConverter
    {
        static BitmapImage onlineIcon = new BitmapImage(new Uri("/Images/Chat.ico", UriKind.Relative));
        static BitmapImage offlineIcon = new BitmapImage(new Uri("/Images/Chat-Offline.ico", UriKind.Relative));
        static BitmapImage awayIcon = new BitmapImage(new Uri("/Images/Chat-Away.ico", UriKind.Relative));
        static BitmapImage busyIcon = new BitmapImage(new Uri("/Images/Chat-Busy.ico", UriKind.Relative));

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is UserStatus)
            {
                var status = (UserStatus)value;
                switch (status)
                {                    
                    case UserStatus.Online:
                        return onlineIcon;
                    case UserStatus.Busy:
                        return busyIcon;
                    case UserStatus.BeRightBack:
                    case UserStatus.Away:
                    case UserStatus.Idle:
                        return awayIcon;
                    case UserStatus.Offline:
                        return offlineIcon;
                    default:
                        return onlineIcon;
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
