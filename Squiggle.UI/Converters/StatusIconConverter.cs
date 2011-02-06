using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Squiggle.Chat;
using Squiggle.UI.Helpers;

namespace Squiggle.UI.Converters
{
    public class StatusIconConverter: IValueConverter
    {
        static BitmapImage onlineIcon, busyIcon, awayIcon, offlineIcon;

        static StatusIconConverter()
        {
            onlineIcon = ImageFactory.Instance.Load(new Uri("pack://siteoforigin:,,,/Images/Status/online.png", UriKind.Absolute));
            busyIcon = ImageFactory.Instance.Load(new Uri("pack://siteoforigin:,,,/Images/Status/busy.png", UriKind.Absolute));
            awayIcon = ImageFactory.Instance.Load(new Uri("pack://siteoforigin:,,,/Images/Status/away.png", UriKind.Absolute));
            offlineIcon = ImageFactory.Instance.Load(new Uri("pack://siteoforigin:,,,/Images/Status/offline.png", UriKind.Absolute));
        }

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
