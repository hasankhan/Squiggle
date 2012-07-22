using System;
using System.Drawing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Squiggle.Chat;
using Squiggle.Core.Presence;
using Squiggle.UI.Helpers;

namespace Squiggle.UI.Converters
{
    public class TrayIconConverter: IMultiValueConverter
    {
        static Icon onlineIcon, offlineIcon, awayIcon, busyIcon;

        static TrayIconConverter()
        {
            onlineIcon = new Icon(Application.GetResourceStream(new Uri("/Images/Chat.ico", UriKind.Relative)).Stream);
            offlineIcon = new Icon(Application.GetResourceStream(new Uri("/Images/Chat-Offline.ico", UriKind.Relative)).Stream);
            awayIcon = new Icon(Application.GetResourceStream(new Uri("/Images/Chat-Away.ico", UriKind.Relative)).Stream);
            busyIcon = new Icon(Application.GetResourceStream(new Uri("/Images/Chat-Busy.ico", UriKind.Relative)).Stream);
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2)
                return null;

            var visibility = values[1] as Visibility?;
            if (values[0] is UserStatus)
            {
                var status = (UserStatus)values[0];

                if (IsBlinking(visibility, status))
                    return null;

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

            return null;
        }        

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        static bool IsBlinking(Visibility? visibility, UserStatus status)
        {
            return status != UserStatus.Offline && visibility == Visibility.Collapsed;
        }
    }
}
