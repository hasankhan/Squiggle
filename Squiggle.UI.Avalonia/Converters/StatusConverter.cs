using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Squiggle.Core.Presence;

namespace Squiggle.UI.Avalonia.Converters;

public class StatusConverter : IValueConverter
{
    public static readonly StatusConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is UserStatus status)
        {
            return status switch
            {
                UserStatus.Online => "Online",
                UserStatus.Busy => "Busy",
                UserStatus.BeRightBack => "Be Right Back",
                UserStatus.Away => "Away",
                UserStatus.Idle => "Idle",
                UserStatus.Offline => "Offline",
                _ => status.ToString()
            };
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
