using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Squiggle.Chat;
using System.Windows.Media.Imaging;

namespace Squiggle.UI
{
    public class StatusIconConverter: IValueConverter
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
                        return new BitmapImage(new Uri("/Images/online.png", UriKind.Relative));
                    case UserStatus.Busy:
                        return new BitmapImage(new Uri("/Images/busy.png", UriKind.Relative));
                    case UserStatus.BeRightBack:
                    case UserStatus.Away:
                    case UserStatus.Idle:
                        return new BitmapImage(new Uri("/Images/away.png", UriKind.Relative));
                    case UserStatus.Offline:
                        return new BitmapImage(new Uri("/Images/offline.png", UriKind.Relative));
                    default:
                        return new BitmapImage(new Uri("/Images/online.png", UriKind.Relative));
                }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
