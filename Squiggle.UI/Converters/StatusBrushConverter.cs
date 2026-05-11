using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Squiggle.Core.Presence;

namespace Squiggle.UI.Converters;

public class StatusBrushConverter : IValueConverter
{
    public static readonly StatusBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is UserStatus status)
        {
            return status switch
            {
                UserStatus.Online => new SolidColorBrush(Color.Parse("#2ECC40")),
                UserStatus.Busy => new SolidColorBrush(Color.Parse("#FF4136")),
                UserStatus.BeRightBack or UserStatus.Away or UserStatus.Idle
                    => new SolidColorBrush(Color.Parse("#FFDC00")),
                UserStatus.Offline => new SolidColorBrush(Color.Parse("#AAAAAA")),
                _ => new SolidColorBrush(Color.Parse("#2ECC40"))
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
