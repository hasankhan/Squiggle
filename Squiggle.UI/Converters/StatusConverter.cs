using System;
using System.Windows;
using System.Windows.Data;
using Squiggle.Chat;
using Squiggle.Core.Presence;
using Squiggle.UI.Resources;

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
                        return Translation.Instance.BuddyStatus_Online;
                    case UserStatus.Busy:
                        return Translation.Instance.BuddyStatus_Busy;
                    case UserStatus.BeRightBack:
                        return Translation.Instance.BuddyStatus_BeRightBack;
                    case UserStatus.Away:
                        return Translation.Instance.BuddyStatus_Away;
                    case UserStatus.Idle:
                        return Translation.Instance.BuddyStatus_Idle;
                    case UserStatus.Offline:
                        return Translation.Instance.BuddyStatus_Offline;
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
